unit MessagesUnit;

interface

const
    {mesaje de eroare}
  StrTranferValidat   = 'Nu puteti transfera o suma deja validata ! Contactati administratorul de casa!';
  StrTranferIncasare  = 'Nu puteti transfera o suma incasata !';
  StrTransferPlata    = 'Nu puteti importa o suma platita !';
  StrTransferCopii    = 'Inregistrarea curenta este o inregistrare compusa ! Nu puteti face transferul decat de pe o pozitie fara defalcare!';
  StrTransferTransfer = 'Inregistrarea curenta este o intrare sau o iesire ! Nu puteti face transferul! ';

  StrAcceptareCopii   = 'Nu puteti valida un copil ! Validarea unui transfer se face numai asupra parintiilor ! ';
  StrAcceptareValidat = 'Nu puteti acceptat o suma deja validata ! Contactati administratorul de casa!';
  StrAcceptarePlati   = 'Nu puteti accepta transferul unei plati !';
  StrAcceptareFail    = 'Nu puteti sa acceptati/rejectati un transfer deja finalizat! ';

  StrValidareCopii    = 'Aceasta inregistrare este un copil! Validare trebuie sa se faca numai pe parinti !';
  StrNonEclExist      = 'Selectia curenta include si note de casa dezechilibrete ! Selectati numai acele note echilibrate!';
  StrValidByAdminS    = 'Una din inregistrariile selectate a fost validata de administrator sau de alt utilizator ! Nu se poate continua validarea !';
  StrValidByAdmin     = 'Inregistrarea a fost validata de administrator sau de alt utilizator ! Nu se poate continua validarea !';

  StrUnValidateCopii  = 'Aceasta inregistrare este un copil! Devalidare trebuie sa se faca numai pe parinti !';
  StrUnValidate       = 'Aceasta inregistrare nu a fost validata. Nu are nici o restrictie !';

  StrNotEchilbrate    = 'Aceasta inregistrare nu este echilibrata! Trebuie mai intati se echilibrati nota pentru a putea genera diferenta !';

  StrShortNotaDiferenta = 'Nota diferenta catre %s pt decont %s din %s pt %s';
  StrNotaDiferenta    = 'Nota de diferenta catre casa %s pentru suma %s justificata de catre %s in casa %s din care s-au cheltuit %s si trebuie returnati un total de : %s';

  StrMoveNotDone      = 'Mutarea nu s-a putut face !';

  StrECLErrorOnParent = 'Nu puteti echilibra decat pozitii din registrul compus !';
  StrErrorOnECLNote   = 'EROARE : nu s-a gasit pozitia : %s din casa curenta !';
  {alte messaje}
  StrLoadRegistru     = 'Se incarca registru principal';
  StrLoadCont         = 'Se incarca defalcarea pe conturi';
  StrLoadProj         = 'Se incarca defalcarea pe proiecte';
  StrLoadFact         = 'Se incarca defalcarea pe facturi';
  StrLoadShare        = 'Se partajeaza tabelele la nivel de server!';

  StrSynRegistru      = 'Se sincronizeaza registrul principal';
  StrSyncCont         = 'Se sincronizeaza defalcarea pe conturi';
  StrSyncProj         = 'Se sincronizeaza defalcarea pe proiecte';

  StrQuestionOnParents = 'Doriti stergrea pozitiilor selectate?';
  StrQuestionOnParent  = 'Doriti stergerea pozitiei din registru?';
  StrQuestionWithChild = 'Doriti stergerea pozitiei compuse?';

  StrQuestionOnDeleteDecont = 'Doriti Stergerea decontului nr: %s din data: %s asociat lui: %s ?'+#13+#10+'Atentie stergerea va duce la pierderea justificarii !';

  StrCancelTransfer    = 'Doriti sa stergeti si inregistrare din casa de destinatie ? ';
  StrWantToSave        = 'Doriti sa salvati Registru de Casa /Banca ? ';

  StrJustHasChildren   = 'Inregistrarea curenta este o inregistrare defalcata ! Nu se poate genera o nota de diferenta!';

  StrJustIsChild       = 'Inregistrarea curenta este o defalcare ! Nu puteti sa Generati diferenta numai din defalcare pe un anumit cont !';

  StrCannotSave        = 'Nu s-au putut salva registrele ! ';
  StrConnectionLost    = 'Conexiunea cu serverul principal a fost pierduta ! Pentru a recupera cea ce ati lucrat inchideti programul si relansati-l !';

  StrNoRepartitorOnDecont = 'Inregistrarea curenta nu are nici un repartitor selectat ! '+#13+#10+'Sunteti sigur ca doriti justificare de avans fara repartitor selectat ? ';

  StrAskForDispozitie = 'Doriti tiparirea unei dispozitii de plata pentru aceasta justificare ?';


implementation

end.
