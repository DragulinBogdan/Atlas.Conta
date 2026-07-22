unit AlopCautare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, cxGraphics,
  cxCheckBox, cxDropDownEdit, cxTextEdit, cxControls, cxContainer, cxEdit,
  cxMaskEdit, cxCalendar, Menus, cxLookAndFeelPainters, cxButtons,
  cxCurrencyEdit, DateUnit, DB, ZDataSet, cxStyles, 
  cxCustomData, cxFilter, cxData, cxDataStorage, cxDBData,
  cxDBLookupComboBox, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView, cxGrid,
  ExtCtrls, cxMemo, cxPC;

type TCode = class(TObject)
  private
       FCod : integer;
  public
      constructor Create(aValue : integer);
      property Cod : integer  read FCod write FCod;
end;

type
  TfrmCautareAlop = class(TForm)
    grpAngajament: TGroupBox;
    aQ: TZQuery;
    Panel2: TPanel;
    btnCautare: TcxButton;
    DataSource1: TDataSource;
    QLichidare: TZQuery;
    DTLichidari: TDataSource;
    QOrdonantare: TZQuery;
    DTOrdonantare: TDataSource;
    QPlata: TZQuery;
    DTPLata: TDataSource;
    pcALOP: TcxPageControl;
    tabLichidare: TcxTabSheet;
    Panel1: TPanel;
    GroupBox1: TGroupBox;
    Splitter1: TSplitter;
    cxGrid2: TcxGrid;
    cxGrid2DBTableView1: TcxGridDBTableView;
    cxGrid2DBTableView1Nr_docum: TcxGridDBColumn;
    cxGrid2DBTableView1tipdoc: TcxGridDBColumn;
    cxGrid2DBTableView1data_docum: TcxGridDBColumn;
    cxGrid2DBTableView1id_predator: TcxGridDBColumn;
    cxGrid2DBTableView1cod_functional: TcxGridDBColumn;
    cxGrid2DBTableView1cod_economic: TcxGridDBColumn;
    cxGrid2DBTableView1suma: TcxGridDBColumn;
    cxGrid2DBTableView1id_primitor: TcxGridDBColumn;
    cxGrid2Level1: TcxGridLevel;
    tabAngajamente: TcxTabSheet;
    cxGrid1: TcxGrid;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1DBTableView1id_alop_angajamente: TcxGridDBColumn;
    cxGrid1DBTableView1data_emitere: TcxGridDBColumn;
    cxGrid1DBTableView1id_departament: TcxGridDBColumn;
    cxGrid1DBTableView1numar: TcxGridDBColumn;
    cxGrid1DBTableView1id_lst_repartitori: TcxGridDBColumn;
    cxGrid1DBTableView1cod_functional: TcxGridDBColumn;
    cxGrid1DBTableView1cod_economic: TcxGridDBColumn;
    cxGrid1DBTableView1id_alop_angajamente_defalcare: TcxGridDBColumn;
    cxGrid1DBTableView1aprobate: TcxGridDBColumn;
    cxGrid1DBTableView1total_angajate: TcxGridDBColumn;
    cxGrid1DBTableView1disponibil: TcxGridDBColumn;
    cxGrid1DBTableView1angajat: TcxGridDBColumn;
    cxGrid1DBTableView1ramas_de_angajat: TcxGridDBColumn;
    cxGrid1Level1: TcxGridLevel;
    tabOrdonantare: TcxTabSheet;
    GroupBox2: TGroupBox;
    cxGrid3: TcxGrid;
    cxGrid3DBTableView1: TcxGridDBTableView;
    cxGrid3DBTableView1id_alop_angajamente: TcxGridDBColumn;
    cxGrid3DBTableView1numar: TcxGridDBColumn;
    cxGrid3DBTableView1data_emitere: TcxGridDBColumn;
    cxGrid3DBTableView1id_departament: TcxGridDBColumn;
    cxGrid3DBTableView1departament: TcxGridDBColumn;
    cxGrid3DBTableView1documente_lichidate: TcxGridDBColumn;
    cxGrid3DBTableView1suma_datorata: TcxGridDBColumn;
    cxGrid3DBTableView1suma_avans: TcxGridDBColumn;
    cxGrid3DBTableView1suma_plata: TcxGridDBColumn;
    cxGrid3DBTableView1id_repartitori: TcxGridDBColumn;
    cxGrid3Level1: TcxGridLevel;
    tabPlata: TcxTabSheet;
    GroupBox3: TGroupBox;
    cxGrid4: TcxGrid;
    cxGrid4DBTableView1: TcxGridDBTableView;
    cxGrid4DBTableView1tipdoc: TcxGridDBColumn;
    cxGrid4DBTableView1nrdoc: TcxGridDBColumn;
    cxGrid4DBTableView1explicatie: TcxGridDBColumn;
    cxGrid4DBTableView1codgest: TcxGridDBColumn;
    cxGrid4DBTableView1cont_csp: TcxGridDBColumn;
    cxGrid4DBTableView1suma: TcxGridDBColumn;
    cxGrid4Level1: TcxGridLevel;
    ADOQuery1: TZQuery;
    DTREP: TDataSource;
    rgCautare: TRadioGroup;
    gbxFactura: TGroupBox;
    ckbNrFct: TcxCheckBox;
    edtNrObligatie: TcxTextEdit;
    Label2: TLabel;
    ckbDataFct: TcxCheckBox;
    edtDataFactura: TcxDateEdit;
    Label3: TLabel;
    gbxAngajamente: TGroupBox;
    ckbFurnizor: TcxCheckBox;
    lblFurnizor: TLabel;
    cbxFurnizor: TcxComboBox;
    ckbDepartament: TcxCheckBox;
    Label1: TLabel;
    cbxDepartament: TcxComboBox;
    ckbDataAngajament: TcxCheckBox;
    edtDataAngajament: TcxDateEdit;
    Label5: TLabel;
    gbxPlata: TGroupBox;
    Label6: TLabel;
    Label7: TLabel;
    ckbNumar: TcxCheckBox;
    edtNrplata: TcxTextEdit;
    ckbData: TcxCheckBox;
    edtPlata: TcxDateEdit;
    ckbFurnizFct: TcxCheckBox;
    Label4: TLabel;
    cbxFurnizFact: TcxComboBox;
    ckbREpartitor: TcxCheckBox;
    Label8: TLabel;
    cbxRepartitor: TcxComboBox;
    cxGrid4DBTableView1Column1: TcxGridDBColumn;
    btnPrintare: TcxButton;
    procedure btnCautareClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure tabLichidareShow(Sender: TObject);
    procedure rgCautareClick(Sender: TObject);
    procedure btnPrintareClick(Sender: TObject);
  private
    { Private declarations }
    procedure UmpleListe;
    procedure LichidareAS1(DataSet: TDataSet);
    procedure LichidareAS0(DataSet: TDataSet);
    procedure LichidareAS2(DataSet : TDataSet);
    procedure aQAS0(DataSet : TDataSet);
    procedure aQAS1(DataSet : TDataSet);
    procedure QPlataAS(DataSet : TDataSet);
  public
    { Public declarations }

  end;

var
  frmCautareAlop : TfrmCautareAlop;
  IsInScroll : boolean;
implementation

{$R *.dfm}

uses
  SetParamsUnitADO, RaportExplorer;

procedure TfrmCautareAlop.btnCautareClick(Sender: TObject);
var
   aCond : string;
begin
  if rgCautare.ItemIndex = 0 then
  begin
//filtre pe angajament departamentul, repartitorul, data
    if (ckbFurnizor.CHecked) and  (cbxFurnizor.ItemIndex >0) then
        aCond := '(ID_LST_REPARTITORI =' +IntTOStr(TCOde(cbxFUrnizor.Properties.Items.Objects[cbxFurnizor.ItemIndex]).cod) +')' ;
    if (ckbDepartament.Checked) and (cbxDepartament.ItemIndex > 0) then
    begin
        if aCOnd <>'' then
          aCOnd := ACond + ' AND ';
       aCond :=aCond + '(ID_DEPARTAMENT ='+ IntTOStr(TCOde(cbxDepartament.Properties.Items.Objects[cbxDepartament.ItemIndex]).cod) +')';
    end;
    if (ckbDataAngajament.Checked) and (edtDataAngajament.Date<>0) then
    begin
        if aCond <>'' then
          aCOnd := ACond + ' AND ';
       aCond :=aCond + ' (DATA_EMITERE = '''+ DateToStr(edtDataAngajament.Date) +''' )';
    end;
    aQ.Close;
    aQ.SQL.Clear;
    aQ.SQL.Text := ' select a.id_alop_angajamente, a.data_emitere, a.id_departament, a.numar, a.id_lst_repartitori, a.cod_functional, '+
                   ' b.cod_economic, b.id_alop_angajamente_defalcare, b.aprobate, b.total_angajate, b.disponibil, b.angajat, b.ramas_de_angajat '+
                   ' from alop_angajamente a '+
                   ' join alop_angajamente_defalcare b on (a.id_alop_angajamente=b.id_alop_angajamente) '+
                   ' order by a.id_lst_repartitori, a.data_emitere,a.id_alop_angajamente ';

    QLichidare.Close;
    QLichidare.SQL.Clear;
    QLichidare.SQL.Text := ' select  a.valoare_receptie_tva as suma , a.cod_economic, a.cod_functional, ' +
                           ' b.data_docum, b.Nr_docum, b.id_primitor, b.id_predator, (select cod_docum from gest_tip_docum where id_gest_tip_docum = b.id_gest_tip_docum) as tipdoc, a.id_gest_itemsi, ' +
                           ' a.id_angajamente_defalcare '+
                           ' from gest_itemsi a '+
                           ' join gest_docum b on (a.id_gest_docum=b.id_gest_docum) and (a.stare=1) and (b.stare=1) '+
                           ' and a.id_angajamente_defalcare =:id_angajamente_defalcare ';
    QOrdonantare.Close;
    QOrdonantare.SQL.Clear;
    QOrdonantare.SQL.Text := ' select id_alop_angajamente, numar, data_emitere, id_departament, departament, '+
                             ' documente_lichidate, suma_datorata, suma_avans, suma_plata, id_repartitori '+
                             ' from alop_ordonantare '+
                             ' where id_alop_angajamente=:id_alop_angajamente ';
    qPlata.Close;
    QPlata.SQL.Clear;
    QPlata.SQL.Text :=  ' select c.tipdoc, c.nrdoc, c.explicatie, c.codgest, c.cont_csp, a.suma,c.data ' +
                        ' from gest_defalcare_decontari a '+
                        ' join gest_decontari b on (a.id_gest_decontari=b.id_gest_decontari) '+
                        ' join bregistru c on (b.id_bregistru=c.cod) '+
                        ' where a.id_gest_itemsi =:id_gest_itemsi ';
    qLichidare.AfterScroll := LichidareAS0;
    aQ.AfterScroll := aqAS0;
    qPlata.AfterScroll := nil;

    aQ.Close;
    aQ.Open;
    aQ.Filter := aCOnd;
    aQ.Filtered := True;
  end;

  if rgCautare.ItemIndex = 1 then // cautare dupa factura
  begin
    if (ckbNrFct.CHecked) and  (edtNrObligatie.Text<>'') then
        aCond := '(Nr_DOCUM =''' + edtNrObligatie.Text +''')' ;
    if (ckbDataFct.Checked)  then
    begin
        if aCOnd <>'' then
          aCOnd := ACond + ' AND ';
       aCond :=aCond + '(DATA_DOCUM ='''+ DateToStr(edtDataFactura.Date) +''')';
    end;
    if (ckbFurnizFct.Checked) and (cbxFurnizFact.ItemIndex > 0) then
    begin
        if aCond <>'' then
          aCOnd := ACond + ' AND ';
       aCond :=aCond + ' (ID_PREDATOR= '+ IntTOStr(TCOde(cbxFurnizFact.Properties.Items.Objects[cbxFurnizFact.ItemIndex]).cod) +' )';
    end;
    qPlata.Close;
    QPlata.SQL.Clear;
    QPlata.SQL.Text :=  ' select c.tipdoc, c.nrdoc, c.explicatie, c.codgest, c.cont_csp, a.suma,c.data, c.cod ' +
                        ' from gest_defalcare_decontari a '+
                        ' join gest_decontari b on (a.id_gest_decontari=b.id_gest_decontari) '+
                        ' join bregistru c on (b.id_bregistru=c.cod) '+
                        ' where a.id_gest_itemsi =:id_gest_itemsi ';
    QOrdonantare.Close;
    QOrdonantare.SQL.Clear;
    QOrdonantare.SQL.Text := ' select id_alop_angajamente, numar, data_emitere, id_departament, departament, '+
                             ' documente_lichidate, suma_datorata, suma_avans, suma_plata, id_repartitori '+
                             ' from alop_ordonantare '+
                             ' where id_alop_angajamente=:id_alop_angajamente ';

    aQ.Close;
    aQ.Params.Clear;
    aQ.SQL.Text :='select a.id_alop_angajamente, a.data_emitere, a.id_departament, a.numar, a.id_lst_repartitori, a.cod_functional, '+
                  'b.cod_economic, b.id_alop_angajamente_defalcare, b.aprobate, b.total_angajate, b.disponibil, b.angajat, b.ramas_de_angajat '+
                  'from alop_angajamente a '+
                  'join alop_angajamente_defalcare b on (a.id_alop_angajamente=b.id_alop_angajamente) and (b.id_alop_angajamente_defalcare = :id_angajamente_defalcare) '+
                  'order by a.id_lst_repartitori, a.data_emitere,a.id_alop_angajamente ';



    QLichidare.Close;
    QLichidare.Sql.Text :='select  a.valoare_receptie_tva as suma , a.cod_economic, a.cod_functional, '+
                  'b.data_docum, b.Nr_docum, b.id_primitor, b.id_predator, (select cod_docum from gest_tip_docum where id_gest_tip_docum = b.id_gest_tip_docum) as tipdoc, a.id_gest_itemsi, '+
                  'a.id_angajamente_defalcare, a.id_gest_docum '+
                  'from gest_itemsi a '+
                  'join gest_docum b on (a.id_gest_docum=b.id_gest_docum) and (a.stare=1) and (b.stare=1) ';

    qPlata.AfterScroll := nil;
    qLichidare.AfterScroll := nil;
    aQ.AfterScroll := nil;


    QLichidare.Open;
    QLichidare.Filter := aCOnd;
    QLichidare.Filtered := True;

    qPlata.AfterScroll := nil;
    qLichidare.AfterScroll := LichidareAS1;
    aQ.AfterScroll := aqAS1;
  end;
  if rgCautare.ItemIndex = 2 then
  begin
//filtre pe angajament departamentul, repartitorul, data
    if (ckbRepartitor.CHecked) and  (cbxRepartitor.ItemIndex >0) then
        aCond := '(CODGEST =' +IntTOStr(TCOde(cbxRepartitor.Properties.Items.Objects[cbxRepartitor.ItemIndex]).cod) +')' ;
    if (ckbNumar.Checked) and (edtNrPlata.Text <> '') then
    begin
        if aCond <>'' then
          aCond := aCond + ' AND ';
       aCond :=aCond + '(NR_DOCUM ='''+ edtNrPlata.Text +''')';
    end;
    if (ckbData.Checked) and (edtPlata.Date<>0) then
    begin
        if aCond <>'' then
          aCond := aCond + ' AND ';
       aCond := aCond + ' (DATA_EMITERE = '''+ DateToStr(edtPlata.Date) +''' )';
    end;
    QPlata.Close;
    QPlata.SQL.Clear;
    QPlata.SQL.Text := ' select c.tipdoc, c.nrdoc, c.explicatie, c.codgest, c.cont_csp, a.suma, a.id_gest_itemsi, c.data, c.cod '+
                       ' from gest_defalcare_decontari a '+
                       ' join gest_decontari b on (a.id_gest_decontari=b.id_gest_decontari) '+
                       ' join bregistru c on (b.id_bregistru=c.cod) ';

    QLichidare.Close;
    QLichidare.SQL.Clear;
    QLichidare.SQL.Text :=' select  a.valoare_receptie_tva as suma , a.cod_economic, a.cod_functional,  ' +
                          ' b.data_docum, b.Nr_docum, b.id_primitor, b.id_predator, (select cod_docum from gest_tip_docum where id_gest_tip_docum = b.id_gest_tip_docum) as tipdoc, a.id_gest_itemsi, ' +
                          ' a.id_angajamente_defalcare, a.id_gest_docum ' +
                          ' from gest_itemsi a '+
                          ' join gest_docum b on (a.id_gest_docum=b.id_gest_docum) and (a.stare=1) and (b.stare=1) '+
                          ' and a.id_gest_itemsi =:id_gest_itemsi ';
    aQ.Close;
    aQ.SQL.Clear;
    aQ.SQL.Text :=  ' select a.id_alop_angajamente, a.data_emitere, a.id_departament, a.numar, a.id_lst_repartitori, a.cod_functional, '+
                    ' b.cod_economic, b.id_alop_angajamente_defalcare, b.aprobate, b.total_angajate, b.disponibil, b.angajat, b.ramas_de_angajat '+
                    ' from alop_angajamente a '+
                    ' join alop_angajamente_defalcare b on (a.id_alop_angajamente=b.id_alop_angajamente) '+
                    ' where b.id_alop_angajamente_defalcare=:id_alop_angajamente_defalcare ' ;

    qOrdonantare.Close;
    QOrdonantare.SQL.Clear;
    QOrdonantare.SQL.Text := ' select id_alop_angajamente, numar, data_emitere, id_departament, departament, '+
                             ' documente_lichidate, suma_datorata, suma_avans, suma_plata, id_repartitori  '+
                             ' from alop_ordonantare '+
                             ' where id_alop_angajamente=:id_alop_angajamente ';

    qLichidare.AfterScroll := nil;
    aQ.AfterScroll := nil;
    QPlata.AfterScroll := nil;

    QPlata.Open;
    QPlata.Filter := aCond;
    QPlata.Filtered := True;

    qLichidare.AfterScroll := LichidareAS2;
    aQ.AfterScroll := aQAS1;
    QPlata.AfterScroll := QPlataAS;
  end;
end;

procedure TfrmCautareAlop.UmpleListe;
var
  tmpq : TZQuery;
begin
  cbxFurnizor.Clear;
  tmpq := GetTmpADOQuery;
  tmpq.SQL.Add('SELECT ID_REPARTITORI, NUME FROM REPARTITORI ORDER BY NUME');
  tmpq.Open;
  while not tmpq.Eof do
  begin
    cbxFurnizor.Properties.Items.AddObject(tmpq.FieldByname('NUME').AsString, TCode.create(tmpq.FieldByName('ID_REPARTITORI').ASInteger));
    tmpQ.Next;
  end;

  cbxDepartament.Clear;
  tmpq.CLose;
  tmpq.SQL.CLear;
  tmpq.SQL.Add('SELECT ID_REPARTITORI, NUME FROM REPARTITORI ORDER BY NUME');  // de pus conditia pe departamente
  tmpq.Open;
  while not tmpq.Eof do
  begin
    cbxDepartament.Properties.Items.AddObject(tmpq.FieldByname('NUME').AsString, TCode.create(tmpq.FieldByName('ID_REPARTITORI').ASInteger));
    tmpQ.Next;
  end;

  cbxFurnizFact.Clear;
  tmpq.CLose;
  tmpq.SQL.CLear;
  tmpq.SQL.Add('SELECT ID_REPARTITORI, NUME FROM REPARTITORI ORDER BY NUME');  // de pus conditia pe departamente
  tmpq.Open;
  while not tmpq.Eof do
  begin
    cbxFurnizFact.Properties.Items.AddObject(tmpq.FieldByname('NUME').AsString, TCode.create(tmpq.FieldByName('ID_REPARTITORI').ASInteger));
    tmpQ.Next;
  end;

  cbxRepartitor.Clear;
  tmpq.CLose;
  tmpq.SQL.CLear;
  tmpq.SQL.Add('SELECT ID_REPARTITORI, NUME FROM REPARTITORI ORDER BY NUME');  // de pus conditia pe departamente
  tmpq.Open;
  while not tmpq.Eof do
  begin
    cbxRepartitor.Properties.Items.AddObject(tmpq.FieldByname('NUME').AsString, TCode.create(tmpq.FieldByName('ID_REPARTITORI').ASInteger));
    tmpQ.Next;
  end;

end;

{ TCode }

constructor TCode.Create(aValue: integer);
begin
    FCod :=aValue;
end;

procedure TfrmCautareAlop.FormCreate(Sender: TObject);
begin
   frmData.QryRepartitori.Open;
   UmpleListe;
   IsInScroll := False;
end;

procedure TfrmCautareAlop.tabLichidareShow(Sender: TObject);
begin
  if not aQ.Active then
    aQ.Open;
end;

procedure TfrmCautareAlop.rgCautareClick(Sender: TObject);
begin
     gbxAngajamente.Visible  :=  (rgCautare.ItemIndex = 0);
     gbxFactura.Visible  :=  (rgCautare.ItemIndex = 1);
     gbxPlata.Visible  :=  (rgCautare.ItemIndex = 2);
     case rgCautare.ItemIndex of
       0 :   pcALOP.ActivePage := tabAngajamente;
       1 :   pcALOP.ActivePage := tabLichidare;
       2 :   pcALOP.ActivePage := tabPlata;
     end;
end;

procedure TfrmCautareAlop.LichidareAS0(DataSet: TDataSet);
begin
   QPlata.Close;
   QPlata.Params.ParamByName('id_gest_itemsi').Value := QLichidare.FieldByName('id_gest_itemsi').AsInteger;
   QPlata.Open;
end;

procedure TfrmCautareAlop.LichidareAS1(DataSet: TDataSet);
begin
   QPlata.Close;
   QPlata.Params.ParamByName('id_gest_itemsi').Value := QLichidare.FieldByName('id_gest_itemsi').AsInteger;
   QPlata.Open;
   aQ.Close;
   aQ.Params.ParamByName('id_angajamente_defalcare').Value := QLichidare.FieldByName('id_angajamente_defalcare').AsInteger;
   aQ.Open;
end;

procedure TfrmCautareAlop.aQAS0(DataSet: TDataSet);
begin
    QOrdonantare.Close;
    QOrdonantare.Params.ParamByName('id_alop_angajamente').Value := aQ.FieldByName('id_alop_angajamente').AsInteger;
    QOrdonantare.Open;
    QLichidare.Close;
    QLichidare.Params.ParamByName('id_angajamente_defalcare').Value :=aQ.FieldByname('id_alop_angajamente_defalcare').AsInteger;
    QLichidare.Open;
end;

procedure TfrmCautareAlop.aQAS1(DataSet: TDataSet);
begin
    QOrdonantare.Close;
    QOrdonantare.Params.ParamByName('id_alop_angajamente').Value := aQ.FieldByName('id_alop_angajamente').AsInteger;
    QOrdonantare.Open;
end;

procedure TfrmCautareAlop.QPlataAS(DataSet: TDataSet);
begin
   qLichidare.Close;
   qLichidare.Params.ParamByName('id_gest_itemsi').Value := qPlata.FIeldByName('id_gest_itemsi').AsInteger;
   qLichidare.Open;
end;


procedure TfrmCautareAlop.LichidareAS2(DataSet: TDataSet);
begin
   aQ.Close;
   aQ.Params.ParamByName('id_alop_angajamente_defalcare').Value := qLichidare.FIeldByName('id_angajamente_defalcare').AsInteger;
   aQ.Open;
end;


procedure TfrmCautareAlop.btnPrintareClick(Sender: TObject);
var
  aQry : TZQuery;
  aIdReport : Integer;
  IdAngajament : integer;
  aStr : String;
begin


 case rgCautare.ItemIndex of
 0 : begin
        aStr := 'UrmarireAngajament';
        aIdReport := -1;
        if aStr <> '' then
          aIdReport :=  DateUnit.GetItemId(aStr);
        IdAngajament :=  aQ.FIeldByName('id_alop_angajamente').AsInteger;
        if aIdReport <> -1 then
        begin
          RegisterCRAdoParam('IdAngajament', IdAngajament);
          LoadReport(aIdReport);
          WriteReportToRepository(aIdReport, 'UrmarireAngajament', IdAngajament);
        end;
    end;
 1 : begin
        aStr := 'UrmarireFactura';
        aIdReport := -1;
        if aStr <> '' then
          aIdReport :=  DateUnit.GetItemId(aStr);
        IdAngajament :=  QLichidare.FieldByName('id_gest_docum').AsInteger;
        if aIdReport <> -1 then
        begin
          RegisterCRAdoParam('IdFactura', IdAngajament);
          LoadReport(aIdReport);
          WriteReportToRepository(aIdReport, 'UrmarireFactura', IdAngajament);
        end;
     end;
 2 : begin
        aStr := 'UrmarirePlata';
        aIdReport := -1;
        if aStr <> '' then
          aIdReport :=  DateUnit.GetItemId(aStr);
        IdAngajament :=  QPlata.FIeldByName('cod').AsInteger;
        if aIdReport <> -1 then
        begin
          RegisterCRAdoParam('IdPlata', IdAngajament);
          LoadReport(aIdReport);
          WriteReportToRepository(aIdReport, 'UrmarirePlata', IdAngajament);
        end;
     end;
 end;
end;


end.



