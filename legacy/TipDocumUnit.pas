unit TipDocumUnit;

interface

uses MemDataSetUnit, Db, Classes;

type

  { Contine lista tipurilor de documente }
  TGrupuriDocument    = class;
  { Contine lista defalcarilor tipurilor de documente }
  TTipuriDocument     = class;
  { Contine lista tipurilor de pozitii pentru defalcarile de tipuri de documente }
  TPozitiiDocument    = class;
  { Contine lista tipurilor de materiale cunoscute de aplicatie }
  TTipuriMaterial     = class;
  { Contine lista tipurilor de terti pentru un anumti tip de document }
  TTipuriTerti        = class;
  { Contine lista tipurilor de coloane pentru un anumit tip de document }
  TListaColoane       = class;
  { Contine formulele de calcul pentru generarea pozitiilor intr-un document conex pe baza pozitiilor dintr-un tip de document }
  TListaFormule       = class;
  { Contine avertismentele care se evalueaza in momentul introducerii }
  TListaAvertismente  = class;
  { Contine lista formulelor de generare note contabile pentru fiecare tip de material }
  TNoteContabile      = class;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

    id_gest_tip_docum   -   integer       =     Identificatorul grupei de document
    cod_docum           -   string        =     Codul grupei de document
    den_docum           -   string        =     Denumirea grupei de document
    desc_docum          -   string        =     Descriere grupei de document
    tip_predator        -   integer       =     Tipul gestiunii permisa la predator
    tip_primitor        -   integer       =     Tipul gestiunii permisa la primitor
    complementeaza_gest -   boolean       =     Daca se complementeaza sau nu gestiunea primitoare in functie de gestiunea predatoare
  }
  TGrupuriDocument = class(TMemDataSet)
  private
    FGrupaDocument: TTipuriDocument;
  protected
    procedure InternalOpen; override;
    procedure InternalClose; override;
    procedure DoAfterScroll; override;
  public
    constructor Create(AOwner: TComponent); override;
    destructor  Destroy; override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

    id_gest_defa_docum  -   integer       =     Identificatorul tipului de document
    predator_intern     -   boolean       =     Predatorul este intern / extern
    primitor_intern     -   boolean       =     Primitorul este intern / extern
    predator_implicit   -   integer       =     Predatorul setat implicit
    primitor_implicit   -   integer       =     Primitorul setat implicit
    numar_automat       -   boolean       =     Daca se genereaza numar de document automat sau nu
    numar_prefix        -   string        =     Prefixul pentru numarul generat
    numar_start         -   integer       =     Inceputul plajei in care se acorda numerele
    numar_end           -   integer       =     Sfarsitul plajei in care se acorda numerele
    id_document_conex   -   integer       =     Identificatorul grupei documentului conex
    copiere_numar_conex -   boolean       =     Daca se copiaza numarul pentru documentul conex (altfel decizia se ia la nivelul documentului conex )
    auto_validare_conex -   boolean       =     Daca se valideaza automat documentul generat (altfel decizia se ia la nivelul documentului conex )
    permite_modificare  -   boolean       =     Daca se permite sau nu modificarea documentului conex inainte de salvare
    predator_conex      -   integer       =     -1 - predatorul de pe documentul original, -2 - primitorul de pe documentul original, > 0 identificatorul predatorului
    primitor_conex      -   integer       =     -1 - predatorul de pe documentul original, -2 - primitorul de pe documentul original, > 0 identificatorul primitorului
    zile_valabilitate   -   integer       =     Durata in zile pentru valabilitate
    zile_expirare       -   integer       =     Durata in zile pentru expirare

    Clasa permite accesul la urmatoarele structuri parinte :
      GrupaDocument     -   TGrupuriDocument        =       clasa care descrie grupa de document din care face parte tipul curent de document
    Clasa permite accesul la urmatoarele structuri subordonate :
      TipPredator       -   TTipuriTerti            =       clasa care descrie tipul gestiunilor suportate ca predatoare
      TipPrimitor       -   TTipuriTerti            =       clasa care descrie tipul gestiunilor suportate ca primitoare

      ColoaneDocument   -   TListaColoane           =       clasa care descrie lista coloanelor afisate (modificabile, valori implicite, etc.) pentru descriere de document
      ColoaneItemsi     -   TListaColoane           =       clasa care descrie lista coloanelor afisate (modificabile, valori implicite, etc.) pentru pozitiile din document

      PozitiiDocument   -   TPozitiiDocument        =       clasa care descrie tipurile de produse suportate si semnele pentru pozitiile de document
  }

  TTipuriDocument = class(TMemDataSet)
  private
    FGrupaDocument   : TGrupuriDocument;
    FTipPredator     : TTipuriTerti;
    FTipPrimitor     : TTipuriTerti;

    FColoaneDocument : TListaColoane;
    FColoaneItemsi   : TListaColoane;

    FPozitiiDocument : TPozitiiDocument;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); virtual;
  public
    property    GrupaDocument   : TGrupuriDocument read FGrupaDocument;
    property    TipPredator     : TTipuriTerti     read FTipPredator;
    property    TipPrimitor     : TTipuriTerti     read FTipPrimitor;
    property    ColoaneDocument : TListaColoane    read FColoaneDocument;
    property    ColoaneItemsi   : TListaColoane    read FColoaneItemsi;
    property    PozitiiDocument : TPozitiiDocument read FPozitiiDocument;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      id_gest_tip_pozitie_docum   -     integer       =   Reprezinta identificatorul tipului de pozitie
      id_gest_tip_produse         -     integer       =   Reprezinta identificatorul tipului de produs
      semn_items                  -     integer       =   reprezinta semnul pozitiei din document ( -1, 0, +1)
      codmat_nou                  -     boolean       =   Daca se genereaza sau nu codmat nou pentru pozitia curenta
      generare_doc_conex          -     boolean       =   Daca pozitia curenta se trece sau nu pe documentul conex generat
                                                          Pentru generare document are prioritate flagul din TipDocument
      tip_stock_predator          -     integer       =   0 nu se executa, > 0 id-ul tipului de stock executat la predator
      tip_stock_primitor          -     integer       =   0 nu se executa, > 0 id-ul tipului de stock executat la primitor
      semn_stock_predator         -     integer       =   semnul cu care se inmulteste stock-ul executat la predator (in cazul in care acesta se executa )
      semn_stock_primitor         -     integer       =   semnul cu care se inmulteste stock-ul executat la primitor (in cazul in care acesta se executa )

  }

  TDetaliiTipDocum = class(TMemDataSet)
  private
    //FTipDocument: TTipuriDocument;
  public
    constructor Create(AOwner: TComponent); override;
  end;

  TPozitiiDocument = class(TMemDataSet)
  protected
    FTipDocument: TTipuriDocument;
    FFormuleConex  : TListaFormule;
    FTipuriMaterial: TTipuriMaterial;

  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      id_repartitori              -     integer       =    Identificatorul tertului
      id_gest_defa_docum          -     integer       =    Identificatorul tipului de document
      id_tip_repartitor           -     integer       =    Identificatorul tipului de repartitor
      nume                        -     string        =    Denumirea tertului
      gestint                     -     boolean       =    DAca este gestiune interna sau externa

  }

  TTipuriTerti = class(TMemDataSet)
  protected
    FTipDocument : TTipuriDocument;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      field_name                  -     string        =     Numele campului asociat coloanei
      caption                     -     string        =     Captura asociata coloanei
      visibil                     -     boolean       =     Daca este vizibil sau nu
      readonly                    -     boolean       =     Daca se permite sau nu modificarea campului
      required                    -     boolean       =     Daca este obligatoriu sa fie completat
      width                       -     integer       =     Dimensiunea implicita de afisare
      color                       -     integer       =     culoarea de fundal
      font_color                  -     integer       =     culoarea fontului

  }

  TListaColoane = class(TMemDataSet)
  protected
    FTipDocument: TTipuriDocument;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      field_name                  -     string        =     Numele campului in care se stocheaza rezultatul formulei de calcul
      formula                     -     string        =     Formula de calcul pentru campul curent
      default_value               -     string        =     Formula de calcul pentru valoare implicita a campului (completata automat in momentul in care se adauga o inregistrare noua )

  }

  TListaFormule = class(TMemDataSet)
  protected
    FPozitiiDocument: TPozitiiDocument;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      field_name                  -     string        =     Numele campului in care se stocheaza rezultatul formulei de calcul
      prioritate                  -     integer       =     Prioritatea in care se executa lista de avertismente
      expresie                    -     string        =     Expresia care se evalueaza pentru a genera avertismentul sau nu
      tip_avertisment             -     string        =     1 - warning, 2 - avertisment visual font/culoare, 3 - avertisment visual (semn de eroare), 4 - eroare

  }

  TListaAvertismente = class(TMemDataSet)
  protected
    FPozitiiDocument: TPozitiiDocument;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      id_gest_tip_material          -    integer       =    Identificatorul tipului de material
      id_gest_tip_pozitie_docum     -    integer       =    Identificatorul pozitie din cadrul tipului de document
      denumire                      -    string        =    Denumirea tipului de material
  }

  TTipuriMaterial = class(TMemDataSet)
    FPozitiiDocument: TPozitiiDocument;
    FNoteContabile  : TNoteContabile;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

  {
    Clasa permite accesul prin intermediul metodei FieldByName la urmatoarele proprietatii ale grupei de document :

      id_gest_tip_material          -    integer       =    Identificatorul tipului de material
      id_gest_tip_pozitie_docum     -    integer       =    Identificatorul pozitie din cadrul tipului de document
      cont_debitor                  -    string        =    Contul debitor pentru nota generata
      cont_creditor                 -    string        =    Contul creditor pentru nota generata
      tert_debit                    -    integer       =     -1 - predatorul de pe documentul original, -2 - primitorul de pe documentul original, > 0 identificatorul predatorului
      tert_credit                   -    integer       =     -1 - predatorul de pe documentul original, -2 - primitorul de pe documentul original, > 0 identificatorul primitorului

  }


  TNoteContabile  = class(TMemDataSet)
  protected
    FTipMaterial : TTipuriMaterial;
  public
    constructor Create(AOwner: TComponent); override;
    procedure   InternalFilter(var Accept: Boolean); override;
  end;

implementation

uses dxmdaset;

{ TGrupuriDocument }

constructor TGrupuriDocument.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  SQLOrder.Add('exec spGetGrupaDocumente');
  FGrupaDocument := TTipuriDocument.Create(Self);
  FGrupaDocument.SetSubComponent(True);
end;

destructor TGrupuriDocument.Destroy;
begin
  inherited Destroy;
end;

procedure TGrupuriDocument.DoAfterScroll;
begin
  FGrupaDocument.UpdateFilters;
  inherited DoAfterScroll;
end;

procedure TGrupuriDocument.InternalClose;
begin
  FGrupaDocument.Close;
  inherited InternalClose;
end;

procedure TGrupuriDocument.InternalOpen;
begin
  inherited InternalOpen;
  FGrupaDocument.Open;
end;

{ TTipuriDocument }

constructor TTipuriDocument.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  SQLOrder.Add('exec spGetTipuriDocumente');
  FGrupaDocument   := TGrupuriDocument(AOwner);
  FPozitiiDocument := TPozitiiDocument.Create(Self);
  FPozitiiDocument.SetSubComponent(True);
end;

procedure TTipuriDocument.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_TIP_DOCUM').AsInteger = FGrupaDocument.FieldByName('ID_GEST_TIP_DOCUM').AsInteger;
end;

{ TPozitiiDocument }

constructor TPozitiiDocument.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  SQLOrder.Add('exec spGetTipuriPozitiiDocument');
  FTipDocument := TTipuriDocument(AOwner);
end;

procedure TPozitiiDocument.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger = FTipDocument.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
end;

{ TTipuriTerti }

constructor TTipuriTerti.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FTipDocument := TTipuriDocument(AOwner);
end;

procedure TTipuriTerti.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger = FTipDocument.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
end;

{ TListaColoane }

constructor TListaColoane.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FTipDocument := TTipuriDocument(AOwner);
end;

procedure TListaColoane.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger = FTipDocument.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
end;

{ TListaFormule }

constructor TListaFormule.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FPozitiiDocument := TPozitiiDocument(AOwner);
end;

procedure TListaFormule.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_TIP_POZITIE_DOCUM').AsInteger = FPozitiiDocument.FieldByName('ID_GEST_TIP_POZITIE_DOCUM').AsInteger;
end;

{ TNoteContabile }

constructor TNoteContabile.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  SQLOrder.Add('exec spGetContariDocumente');
  FTipMaterial := TTipuriMaterial(AOwner);
end;

procedure TNoteContabile.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_TIP_PRODUSE').AsInteger = FTipMaterial.FieldByName('ID_GEST_TIP_PRODUSE').AsInteger;
end;

{ TListaAvertismente }

constructor TListaAvertismente.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FPozitiiDocument := TPozitiiDocument(AOwner);
end;

procedure TListaAvertismente.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('ID_GEST_TIP_POZITIE_DOCUM').AsInteger = FPozitiiDocument.FieldByName('ID_GEST_TIP_POZITIE_DOCUM').AsInteger;
end;

{ TTipuriMaterial }

constructor TTipuriMaterial.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FPozitiiDocument := TPozitiiDocument(AOwner);
  FNoteContabile   := TNoteContabile.Create(Self);
  FNoteContabile.SetSubComponent(True);
end;

procedure TTipuriMaterial.InternalFilter(var Accept: Boolean);
begin
  Accept := FieldByName('').AsInteger = FPozitiiDocument.FieldByName('').AsInteger;
end;

{ TDetaliiTipDocum }

constructor TDetaliiTipDocum.Create(AOwner: TComponent);
begin
  inherited;

end;

end.
