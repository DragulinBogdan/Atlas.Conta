"""Creează clone de test sufixate; nu șterge baze și nu întrerupe conexiuni."""
import argparse
import re

import psycopg
from psycopg import sql


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sufix", required=True)
    parser.add_argument("--profil", choices=["Privat", "Bugetar", "Ambele"], default="Ambele")
    args = parser.parse_args()
    if not re.fullmatch(r"[._][A-Za-z0-9_-]{1,24}", args.sufix):
        parser.error("Sufixul trebuie să înceapă cu . sau _ și să aibă 1–24 caractere alfanumerice, _ sau -.")
    sources = {
        "Bugetar": "Atlas.Conta.BackOffice",
        "Privat": "Atlas.Conta.ModelCheck.Privat",
    }
    with psycopg.connect(
        host="127.0.0.1", port=5446, user="postgres", password="postgres",
        dbname="postgres", autocommit=True, connect_timeout=10,
    ) as conn:
        for profile, source in sources.items():
            if args.profil not in (profile, "Ambele"):
                continue
            target = source + args.sufix
            if conn.execute("SELECT 1 FROM pg_database WHERE datname = %s", (target,)).fetchone():
                print(f"EXISTĂ {profile}: {target}; păstrată")
                continue
            if not conn.execute("SELECT 1 FROM pg_database WHERE datname = %s", (source,)).fetchone():
                raise RuntimeError(f"Lipsește baza de profil {source}; pregătește seed-ul înaintea clonării.")
            conn.execute(sql.SQL("CREATE DATABASE {} WITH TEMPLATE {}").format(
                sql.Identifier(target), sql.Identifier(source)))
            print(f"CREATĂ {profile}: {target}; clonă din {source}")


if __name__ == "__main__":
    main()
