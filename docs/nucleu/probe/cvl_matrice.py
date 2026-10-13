"""Arithmetic model for the CV proposal. Does not execute Atlas.Conta."""
import argparse
import hashlib
import itertools
import json
from dataclasses import asdict, dataclass
from pathlib import Path


@dataclass(frozen=True)
class Row:
    id: str
    transaction: str
    kind: str
    owner: str
    cause: str
    pair: int
    account: str
    side: int
    quantity: int
    value: int
    day: int
    assigned: str | None = None
    inverse: str | None = None


def reverse(rows, command, day):
    return [Row('i' + r.id, command, 'Storno', command.removeprefix('S'), r.cause,
                r.pair, r.account, r.side, -r.quantity, -r.value,
                day, r.assigned, r.id) for r in rows]


def balances(rows, day=31):
    visible = [r for r in rows if r.day <= day]
    return [sum(r.quantity for r in visible if r.account == 'S')] + [
        sum(r.side * r.value for r in visible if r.account == a) * sign
        for a, sign in [('S', 1), ('C', 1), ('T', -1)]]


def check(rows, originals):
    for tx in {r.transaction for r in rows}:
        group = [r for r in rows if r.transaction == tx]
        assert sum(r.side * r.value for r in group) == 0
        assert sum(r.quantity for r in group) == 0
        for pair in {r.pair for r in group}:
            a, b = [r for r in group if r.pair == pair]
            assert a.side == -b.side and a.value == b.value
            assert a.quantity == -b.quantity
        for r in group:
            if r.kind == 'Storno':
                o = originals[r.inverse]
                assert o.kind != 'Storno'
                assert (r.account, r.side, r.cause, r.assigned) == (
                    o.account, o.side, o.cause, o.assigned)
                assert (r.quantity, r.value) == (-o.quantity, -o.value)


def fixture(deltas):
    rows = [Row('f1', 'F', 'Operare', 'F', 'F', 1, 'S', 1, 100, 1000, 5),
            Row('f2', 'F', 'Operare', 'F', 'F', 1, 'T', -1, -100, 1000, 5),
            Row('b1', 'B', 'Operare', 'B', 'B', 1, 'C', 1, 60, 600, 10),
            Row('b2', 'B', 'Operare', 'B', 'B', 1, 'S', -1, -60, 600, 10)]
    for i, delta in enumerate(deltas):
        key = 'A' + str(i)
        for n, account, side, value, assigned in [
                (1, 'S', 1, delta * 4 // 10, 'f1'),
                (2, 'T', -1, delta * 4 // 10, None),
                (3, 'C', 1, delta * 6 // 10, 'b1'),
                (4, 'T', -1, delta * 6 // 10, None)]:
            rows.append(Row(key + str(n), key, 'Operare', key, key,
                            (n + 1) // 2, account, side, 0, value, 15 + i, assigned))
    return rows


def simulate(deltas, order, days):
    rows = fixture(deltas)
    initial = list(rows)
    active = set('A' + str(i) for i in range(len(deltas)))
    compensations = {}
    b_active = True
    f_active = True
    steps = []
    for command, day in zip(order, days, strict=True):
        # The fixture has one connected family on a single lot.
        if day < max(r.day for r in rows):
            steps.append({'command': command, 'day': day, 'refusal': 'CV_CRONOLOGIE'})
            continue
        if command == 'F':
            # Reception storno: quantity guard first, then active correction value on stock.
            refusal = 'STOC_INSUFICIENT' if b_active else 'CV_DEPENDENTI' if active else None
            if refusal:
                steps.append({'command': command, 'day': day, 'refusal': refusal})
                continue
            batch = reverse([r for r in initial if r.owner == 'F'], 'SF', day)
            f_active = False
        elif command == 'B':
            batch = reverse([r for r in initial if r.owner == 'B'], 'SB', day)
            # One extra Operare per correction document; cause and owner are the correction.
            for key in sorted(active):
                delta = deltas[int(key[1:])]
                k = [Row('k' + key + str(n), 'K' + key, 'Operare', key, key,
                         1, account, side, 0, -delta * 6 // 10, day, key + '3')
                     for n, account, side in [(1, 'S', -1), (2, 'C', 1)]]
                compensations[key] = k
                batch += k
            b_active = False
        else:
            sources = [r for r in initial if r.owner == command]
            # Logical pairing only; production remaps by source transaction Guid.
            inverses = reverse(sources, 'S' + command, day)
            for r in reverse(compensations.get(command, []), 'S' + command, day):
                inverses.append(Row(**(asdict(r) | {'pair': 3})))
            batch = inverses
            active.remove(command)
        check(batch, {r.id: r for r in rows})
        rows += batch
        delta = sum(deltas[int(key[1:])] for key in active)
        q, v = (100, 1000) if f_active else (0, 0)
        expected = ([40, 400 + delta * 4 // 10, 600 + delta * 6 // 10, 1000 + delta]
                    if b_active else [q, v + delta, 0, v + delta])
        assert balances(rows) == expected
        # Emulate only the observed Contabil.Postari inclusion rule.
        by_id = {r.id: r for r in rows}
        accounting = [r for r in rows if r.kind == 'Operare' or
                      r.inverse and by_id[r.inverse].kind == 'Operare']
        assert balances(accounting) == expected
        assert all(r.cause == r.owner for r in batch)
        for cause in {r.cause for r in rows}:
            s, c, t = (sum(r.side * r.value for r in rows if r.cause == cause and r.account == a)
                       for a in 'SCT')
            if cause == 'F':
                assert (s, c, t) == ((1000, 0, -1000) if f_active else (0, 0, 0))
            elif cause == 'B':
                assert (s, c, t) == ((-600, 600, 0) if b_active else (0, 0, 0))
            else:
                delta = deltas[int(cause[1:])] if cause in active else 0
                assert (s + c, t) == (delta, -delta)
                if not b_active:
                    assert c == 0
        steps.append({'command': command, 'day': day, 'balance': expected,
                      'posted': [asdict(r) for r in batch]})
    return {'deltas': deltas, 'steps': steps, 'at22': balances(rows, 22),
            'final': balances(rows)}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', required=True, type=Path)
    args = parser.parse_args()
    cases = []
    for deltas in ([200], [-200], [200, 100], [200, -100]):
        for order in itertools.permutations(['B'] + ['A' + str(i) for i in range(len(deltas))]):
            result = simulate(deltas, order, range(20, 20 + len(order)))
            assert result['final'] == [100, 1000, 0, 1000]
            cases.append(result)
    temporal = [simulate([200], order, days) for order, days in [
        (['A0', 'B'], [25, 20]), (['B'], [14]),
        (['B', 'A0'], [25, 20]), (['A0', 'B'], [25, 25])]]
    assert all(x['steps'][-1].get('refusal') == 'CV_CRONOLOGIE' for x in temporal[:3])
    assert temporal[0]['at22'] == temporal[2]['at22'] == [40, 480, 720, 1200]
    assert temporal[3]['final'] == [100, 1000, 0, 1000]
    reception = [simulate([200], order, days) for order, days in [
        (['F'], [20]), (['B', 'F'], [20, 21]),
        (['A0', 'B', 'F'], [20, 21, 22]), (['B', 'A0', 'F'], [20, 21, 22]),
        (['A0', 'B', 'F'], [25, 25, 20])]]
    assert [x['steps'][-1].get('refusal') for x in reception] == [
        'STOC_INSUFICIENT', 'CV_DEPENDENTI', None, None, 'CV_CRONOLOGIE']
    assert reception[2]['final'] == reception[3]['final'] == [0, 0, 0, 0]
    assert {r['id'] for r in reception[3]['steps'][-1]['posted']} == {'if1', 'if2'}
    artifact = {'scope': 'Arithmetic design model, not Atlas.Conta execution',
                'scriptSha256': hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
                'orders': cases, 'temporal': temporal, 'reception': reception}
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(artifact, ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'OK: {len(cases)} orders, {len(temporal)} temporal cases, '
          f'{len(reception)} reception cases; {args.out}')


if __name__ == '__main__':
    main()
