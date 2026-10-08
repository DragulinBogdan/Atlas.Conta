-- D9-A1: scara de volum. Secțiunile se rulează pe nume din `ScaraVolum.cs`; identitatea copiei c e scara.h(vechi, c).
-- Sursa copiilor e fotografia scenei din schema `scara`; tabelele din `public` rămân cele ale codului.

-- @@ fotografie
create schema scara;
create function scara.h(vechi uuid, c integer) returns uuid language sql immutable parallel safe
    return md5(vechi::text || ':' || c)::uuid;
create table scara.tranzactie as select * from "Tranzactie";
create table scara.postare as select * from "Postare";
create table scara.document as select * from "Documente";
create table scara.lot as select * from "Loturi";
create table scara.partener as
    select r.* from "Repartitori" r
    where r."ClrType" = 'Partener'
      and r."ID" in (select p."Partener" from "Postare" p where p."Partener" is not null);
alter table scara.partener add primary key ("ID");
-- Regula din `Partide.Origini`: postarea al cărei document dă identitatea unității.
create table scara.partida as
    select distinct p."Unitate" as unitate, p."DocumentId" as document, p."Cont" as cont, p."Partener" as partener
    from "Postare" p
    where p."FelUnitate" = 2 and p."DocumentId" is not null and p."Partener" is not null
      and p."Unitate" = cub_partida_id(p."DocumentId", p."Cont", p."Partener");
alter table scara.partida add primary key (unitate);
create table scara.partida_deschidere as
    select distinct p."Unitate" as unitate
    from "Postare" p join "Tranzactie" t on t."ID" = p."TranzactieId"
    where p."FelUnitate" = 2 and p."DocumentId" is null and t."Fel" = 4;
alter table scara.partida_deschidere add primary key (unitate);

-- @@ copiaza
insert into "Repartitori" ("ID", "ClrType", "Cod", "Denumire", "Calitati", "Activ", "ContImplicitId", "Marca", "Iban", "EsteBanca",
        "CodFiscal", "RegistruComert", "TipPersoana", "Tara", "InregistratTva", "TvaLaIncasare", "TipTvaImplicitId", "Strada", "Numar",
        "DetaliiAdresa", "Localitate", "CodPostal", "JudetId", "DataSincronizareAnaf", "InactivFiscal", "OptimisticLockField")
select scara.h(r."ID", k.c), r."ClrType", r."Cod" || '#' || k.c, r."Denumire", r."Calitati", r."Activ", r."ContImplicitId", r."Marca", r."Iban",
       r."EsteBanca", r."CodFiscal", r."RegistruComert", r."TipPersoana", r."Tara", r."InregistratTva", r."TvaLaIncasare", r."TipTvaImplicitId",
       r."Strada", r."Numar", r."DetaliiAdresa", r."Localitate", r."CodPostal", r."JudetId", r."DataSincronizareAnaf", r."InactivFiscal",
       r."OptimisticLockField"
from scara.partener r cross join generate_series(@de_la, @pana_la - 1) k(c);

insert into "Documente" ("ID", "ClrType", "Numar", "Data", "DataInregistrare", "PredatorId", "PrimitorId", "Stare", "DataOperare",
        "DocumentSursaId", "Autogenerat", "CorecteazaId", "MotivCorectie", "NumarPV", "DataPV", "TipInstrument",
        "NumarExtras", "DataExtras", "LaturaPerecheId", "DataScadenta", "GestiuneDescarcareId", "CodCpv", "TethysId", "Valuta", "Curs",
        "GenereazaPlata", "PlataContPropriuId", "PlataNumar", "PlataData", "PlataTipInstrument", "GenereazaChitanta", "ChitantaNumar",
        "ChitantaData", "Cauza", "SursaReceptieiId", "TranzactieReceptieSursaId", "OptimisticLockField", "DataExigibilitate", "DataPrimire")
select scara.h(d."ID", k.c), d."ClrType", d."Numar", d."Data", d."DataInregistrare",
       case when pp."ID" is not null then scara.h(d."PredatorId", k.c) else d."PredatorId" end,
       case when pm."ID" is not null then scara.h(d."PrimitorId", k.c) else d."PrimitorId" end,
       d."Stare", d."DataOperare", scara.h(d."DocumentSursaId", k.c), d."Autogenerat", scara.h(d."CorecteazaId", k.c), d."MotivCorectie",
       d."NumarPV", d."DataPV", d."TipInstrument", d."NumarExtras", d."DataExtras", scara.h(d."LaturaPerecheId", k.c),
       d."DataScadenta", d."GestiuneDescarcareId", d."CodCpv", d."TethysId", d."Valuta", d."Curs", d."GenereazaPlata", d."PlataContPropriuId",
       d."PlataNumar", d."PlataData", d."PlataTipInstrument", d."GenereazaChitanta", d."ChitantaNumar", d."ChitantaData", d."Cauza",
       scara.h(d."SursaReceptieiId", k.c), scara.h(d."TranzactieReceptieSursaId", k.c), d."OptimisticLockField", d."DataExigibilitate",
       d."DataPrimire"
from scara.document d
left join scara.partener pp on pp."ID" = d."PredatorId"
left join scara.partener pm on pm."ID" = d."PrimitorId"
cross join generate_series(@de_la, @pana_la - 1) k(c);

insert into "Loturi" ("ID", "ProdusId", "PretUnitar", "GestiuneId", "Data", "DataExpirare", "LotFabricatie", "LinieIntrareId", "OptimisticLockField")
select scara.h(l."ID", k.c), l."ProdusId", l."PretUnitar", l."GestiuneId", l."Data", l."DataExpirare", l."LotFabricatie", l."LinieIntrareId",
       l."OptimisticLockField"
from scara.lot l cross join generate_series(@de_la, @pana_la - 1) k(c);

-- `IX_Tranzactie_Fel` admite o singură tranzacție `Deschidere` (Fel = 4): nu se clonează, postările ei clonate rămân pe ea.
insert into "Tranzactie" ("ID", "DocumentId", "Fel", "Data", "ScrisLa", "Explicatie", "ExplicatieDinId")
select scara.h(t."ID", k.c), scara.h(t."DocumentId", k.c), t."Fel", t."Data", t."ScrisLa", null, null
from scara.tranzactie t cross join generate_series(@de_la, @pana_la - 1) k(c)
where t."Fel" <> 4;

with copii as materialized (select c from generate_series(@de_la, @pana_la - 1) k(c)),
partide as materialized (
    select o.unitate, k.c,
           cub_partida_id(scara.h(o.document, k.c), o.cont,
               case when pa."ID" is not null then scara.h(o.partener, k.c) else o.partener end) as unitate_noua
    from scara.partida o left join scara.partener pa on pa."ID" = o.partener cross join copii k)
insert into "Postare" ("ID", "Spatiu", "TranzactieId", "DocumentId", "LinieId", "Data", "Cont", "Latura", "Partener", "Gestiune", "Produs",
        "Unitate", "UnitateDeschisa", "FelUnitate", "SuportId", "SuportSpatiu", "InversaDinId", "InversaDinSpatiu", "TipTvaId", "SensTva",
        "RolTva", "PerioadaDeclarare", "Valuta", "Carte", "CodFunctional", "CodEconomic", "SursaFinantare", "UnitateOrganizatorica", "Proiect",
        "CentruCost", "Atribuit", "Cantitate", "ValoareValuta", "Valoare", "CotaTva", "DataDocument", "DataExigibilitate", "DataInregistrare",
        "DataPrimire", "DeImport", "DocumentFiscalId", "InversaTehnica", "PerioadaD394", "RegimTva", "RegularizareD300", "Pereche")
select scara.h(p."ID", k.c), p."Spatiu",
       case when t."Fel" = 4 then p."TranzactieId" else scara.h(p."TranzactieId", k.c) end,
       scara.h(p."DocumentId", k.c), p."LinieId", p."Data", p."Cont", p."Latura",
       case when pa."ID" is not null then scara.h(p."Partener", k.c) else p."Partener" end,
       p."Gestiune", p."Produs",
       case when p."FelUnitate" = 1 then scara.h(p."Unitate", k.c)
            when p."FelUnitate" = 2 then coalesce(o.unitate_noua, scara.h(p."Unitate", k.c))
            else p."Unitate" end,
       p."UnitateDeschisa", p."FelUnitate", scara.h(p."SuportId", k.c), p."SuportSpatiu", scara.h(p."InversaDinId", k.c), p."InversaDinSpatiu",
       p."TipTvaId", p."SensTva", p."RolTva", p."PerioadaDeclarare", p."Valuta", p."Carte", p."CodFunctional", p."CodEconomic",
       p."SursaFinantare", p."UnitateOrganizatorica", p."Proiect", p."CentruCost", p."Atribuit", p."Cantitate", p."ValoareValuta", p."Valoare",
       p."CotaTva", p."DataDocument", p."DataExigibilitate", p."DataInregistrare", p."DataPrimire", p."DeImport",
       scara.h(p."DocumentFiscalId", k.c), p."InversaTehnica", p."PerioadaD394", p."RegimTva", p."RegularizareD300", p."Pereche"
from scara.postare p
cross join copii k
join scara.tranzactie t on t."ID" = p."TranzactieId"
left join scara.partener pa on pa."ID" = p."Partener"
left join partide o on o.unitate = p."Unitate" and o.c = k.c and p."FelUnitate" = 2;

-- @@ probe
-- (proba, cheie, scena, baza, factor): factor 'f' cere baza = f × scena, '1' cere baza = scena.
select 'postări pe Spatiu × Carte', s."Spatiu" || '/' || s."Carte", s.n::numeric, coalesce(b.n, 0)::numeric, 'f'
from (select "Spatiu", "Carte", count(*) n from scara.postare group by 1, 2) s
left join (select "Spatiu", "Carte", count(*) n from "Postare" group by 1, 2) b using ("Spatiu", "Carte")
union all
select 'tranzacții pe Fel', s."Fel"::text, s.n, coalesce(b.n, 0), case when s."Fel" = 4 then '1' else 'f' end
from (select "Fel", count(*) n from scara.tranzactie group by 1) s
left join (select "Fel", count(*) n from "Tranzactie" group by 1) b using ("Fel")
union all
select 'Σ Valoare pe latură', s."Latura"::text, s.v, coalesce(b.v, 0), 'f'
from (select "Latura", sum("Valoare") v from scara.postare group by 1) s
left join (select "Latura", sum("Valoare") v from "Postare" group by 1) b using ("Latura")
union all
select 'Σ Cantitate pe spațiu', s."Spatiu"::text, s.q, coalesce(b.q, 0), 'f'
from (select "Spatiu", sum("Cantitate") q from scara.postare group by 1) s
left join (select "Spatiu", sum("Cantitate") q from "Postare" group by 1) b using ("Spatiu")
union all
select 'documente', '', (select count(*) from scara.document), (select count(*) from "Documente"), 'f'
union all
select 'loturi', '', (select count(*) from scara.lot), (select count(*) from "Loturi"), 'f'
union all
select 'parteneri purtați de postări', '', (select count(*) from scara.partener),
       (select count(*) from "Repartitori" r join scara.partener s on r."Cod" = s."Cod" or r."Cod" like s."Cod" || '#%'), 'f'
union all
select 'partide ale scenei fără origine și fără deschidere', '', 0,
       (select count(*) from (select distinct "Unitate" u from scara.postare where "FelUnitate" = 2) x
        where not exists (select 1 from scara.partida o where o.unitate = x.u)
          and not exists (select 1 from scara.partida_deschidere d where d.unitate = x.u)), '1'
union all
select 'postări cu inversa sau suportul fără postare', '', 0,
       (select count(*) from "Postare" p
        where p."InversaDinId" is not null and not exists (select 1 from "Postare" x where x."Spatiu" = p."InversaDinSpatiu" and x."ID" = p."InversaDinId")
           or p."SuportId" is not null and not exists (select 1 from "Postare" x where x."Spatiu" = p."SuportSpatiu" and x."ID" = p."SuportId")), '1'
union all
select 'postări fiscale cu documentul fiscal lipsă', '', 0,
       (select count(*) from "Postare" p where p."DocumentFiscalId" is not null
          and not exists (select 1 from "Documente" d where d."ID" = p."DocumentFiscalId")), '1'
order by 1, 2;

-- @@ probe_copie
-- Copia @c, rând cu rând: tot ce nu e rescris e identic cu originalul.
select 'copia: Postare identică în afara coloanelor rescrise', '', count(*)::numeric,
       count(*) filter (where to_jsonb(n.*) - array['ID', 'TranzactieId', 'DocumentId', 'DocumentFiscalId', 'InversaDinId', 'SuportId', 'Partener', 'Unitate']
           = to_jsonb(p.*) - array['ID', 'TranzactieId', 'DocumentId', 'DocumentFiscalId', 'InversaDinId', 'SuportId', 'Partener', 'Unitate'])::numeric, '1'
from scara.postare p left join "Postare" n on n."Spatiu" = p."Spatiu" and n."ID" = scara.h(p."ID", @c)
union all
select 'copia: Tranzactie identică în afara coloanelor rescrise', '', count(*),
       count(*) filter (where to_jsonb(n.*) - array['ID', 'DocumentId', 'Explicatie', 'ExplicatieDinId']
           = to_jsonb(t.*) - array['ID', 'DocumentId', 'Explicatie', 'ExplicatieDinId'] and n."Explicatie" is null and n."ExplicatieDinId" is null), '1'
from scara.tranzactie t left join "Tranzactie" n on n."ID" = scara.h(t."ID", @c) where t."Fel" <> 4
union all
select 'copia: Documente identice în afara coloanelor rescrise', '', count(*),
       count(*) filter (where to_jsonb(n.*) - array['ID', 'PredatorId', 'PrimitorId', 'DocumentSursaId', 'CorecteazaId', 'LaturaPerecheId', 'SursaReceptieiId', 'TranzactieReceptieSursaId']
           = to_jsonb(d.*) - array['ID', 'PredatorId', 'PrimitorId', 'DocumentSursaId', 'CorecteazaId', 'LaturaPerecheId', 'SursaReceptieiId', 'TranzactieReceptieSursaId']), '1'
from scara.document d left join "Documente" n on n."ID" = scara.h(d."ID", @c)
union all
select 'copia: Loturi identice în afara cheii', '', count(*),
       count(*) filter (where to_jsonb(n.*) - 'ID' = to_jsonb(l.*) - 'ID'), '1'
from scara.lot l left join "Loturi" n on n."ID" = scara.h(l."ID", @c)
union all
select 'copia: parteneri identici în afara cheii și a codului', '', count(*),
       count(*) filter (where to_jsonb(n.*) - array['ID', 'Cod', 'Cautare'] = to_jsonb(r.*) - array['ID', 'Cod', 'Cautare']
           and n."Cod" = r."Cod" || '#' || @c), '1'
from scara.partener r left join "Repartitori" n on n."ID" = scara.h(r."ID", @c)
union all
select 'copia: postările de partidă poartă partenerul clonat', '', count(*),
       count(*) filter (where n."Partener" = scara.h(p."Partener", @c)), '1'
from scara.postare p join scara.partener pa on pa."ID" = p."Partener"
left join "Postare" n on n."Spatiu" = p."Spatiu" and n."ID" = scara.h(p."ID", @c)
union all
select 'copia: unitatea de lot e lotul clonat', '', count(*),
       count(*) filter (where n."Unitate" = scara.h(p."Unitate", @c)), '1'
from scara.postare p left join "Postare" n on n."Spatiu" = p."Spatiu" and n."ID" = scara.h(p."ID", @c)
where p."FelUnitate" = 1
order by 1;

-- @@ forma
select 'postări, spațiul Contabil', count(*)::numeric from "Postare" where "Spatiu" = 1
union all select 'postări, spațiul Stoc', count(*) from "Postare" where "Spatiu" = 2
union all select 'postări în luna măsurată', count(*) from "Postare" where "Data" between @inceput and @sfarsit
union all select 'tranzacții', count(*) from "Tranzactie"
union all select 'documente', count(*) from "Documente"
union all select 'loturi', count(*) from "Loturi"
union all select 'parteneri (Repartitori de tip Partener)', count(*) from "Repartitori" where "ClrType" = 'Partener'
union all select 'partide (unități de fel Partidă)', count(distinct "Unitate") from "Postare" where "FelUnitate" = 2
union all select 'rânduri SolduriPerioadaContabil', count(*) from "SolduriPerioadaContabil"
union all select 'rânduri SolduriPerioadaStoc', count(*) from "SolduriPerioadaStoc"
union all select 'rânduri PartideDeschise', count(*) from "PartideDeschise"
union all select 'octeți Postare_Contabil (tabelă)', pg_table_size('"Postare_Contabil"')
union all select 'octeți Postare_Contabil (indecși)', pg_indexes_size('"Postare_Contabil"')
union all select 'octeți Postare_Stoc (tabelă)', pg_table_size('"Postare_Stoc"')
union all select 'octeți Postare_Stoc (indecși)', pg_indexes_size('"Postare_Stoc"')
union all select 'octeți Tranzactie (cu indecși)', pg_total_relation_size('"Tranzactie"')
union all select 'octeți Documente (cu indecși)', pg_total_relation_size('"Documente"')
union all select 'octeți Loturi (cu indecși)', pg_total_relation_size('"Loturi"')
union all select 'octeți SolduriPerioadaContabil (cu indecși)', pg_total_relation_size('"SolduriPerioadaContabil"')
union all select 'octeți SolduriPerioadaStoc (cu indecși)', pg_total_relation_size('"SolduriPerioadaStoc"')
union all select 'octeți PartideDeschise (cu indecși)', pg_total_relation_size('"PartideDeschise"')
union all select 'octeți baza întreagă', pg_database_size(current_database());

-- @@ configuratie
select 'version()', version()
union all select 'shared_buffers', current_setting('shared_buffers')
union all select 'work_mem', current_setting('work_mem')
union all select 'effective_cache_size', current_setting('effective_cache_size')
union all select 'max_parallel_workers_per_gather', current_setting('max_parallel_workers_per_gather')
union all select 'maintenance_work_mem', current_setting('maintenance_work_mem')
union all select 'random_page_cost', current_setting('random_page_cost')
union all select 'jit', current_setting('jit');
