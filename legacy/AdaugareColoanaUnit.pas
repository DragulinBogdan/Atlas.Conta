unit AdaugareColoanaUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, ExtCtrls, StdCtrls, dxExEdtr, dxEdLib, dxDBELib, dxEditor,
  dxCntner, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmAdaugareColoana = class(TForm)
    DTColoana: TDataSource;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    Label10: TLabel;
    Label11: TLabel;
    Label12: TLabel;
    Label13: TLabel;
    Label14: TLabel;
    Label15: TLabel;
    Label16: TLabel;
    Label19: TLabel;
    Label20: TLabel;
    Label21: TLabel;
    edNumeColoana: TdxDBEdit;
    edtCaption: TdxDBMaskEdit;
    edtTipData: TdxImageEdit;
    edtClasaEditare: TdxDBImageEdit;
    AtsDBSpinEdit1: TdxDBSpinEdit;
    AtsDBSpinEdit2: TdxDBSpinEdit;
    AtsDBSpinEdit3: TdxDBSpinEdit;
    AtsDBImageEdit3: TdxDBImageEdit;
    AtsDBImageEdit4: TdxDBImageEdit;
    AtsDBCurrencyEdit1: TdxDBCurrencyEdit;
    AtsDBCurrencyEdit2: TdxDBCurrencyEdit;
    AtsDBCurrencyEdit3: TdxDBCurrencyEdit;
    AtsDBCheckEdit1: TdxDBCheckEdit;
    AtsDBCheckEdit2: TdxDBCheckEdit;
    AtsDBCheckEdit3: TdxDBCheckEdit;
    AtsDBButtonEdit1: TdxDBButtonEdit;
    Bevel1: TBevel;
    Bevel2: TBevel;
    Bevel3: TBevel;
    Button1: TcxButton;
    Button2: TcxButton;
    Button3: TcxButton;
    procedure Button1Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure edtTipDataChange(Sender: TObject);
  private
    { Private declarations }
    FDataSet: TDataSet;
    FAdaugare: Boolean;
    procedure SetDataSet(const Value: TDataSet);
    procedure SaveData;
  public
    { Public declarations }
    property DataSet: TDataSet read FDataSet write SetDataSet;
    property Adaugare: Boolean read FAdaugare;
  end;

function AdaugaColoanaNoua(ADataSet: TDataSet): Boolean;

implementation

{$R *.DFM}

uses DateUnit, dxDBGrid, CommonDBVar;

function AdaugaColoanaNoua(ADataSet: TDataSet): Boolean;
begin
  with TfrmAdaugareColoana.Create(Application) do
    try
       FDataSet := ADataSet;
       DTColoana.DataSet := FDataSet;
       FAdaugare := True;
       FDataSet.Edit;
       if not ShowModal = mrOk then FDataSet.Cancel;
    finally
       Free;
    end;
end;

{ TfrmAdaugareColoana }

procedure TfrmAdaugareColoana.SaveData;
begin
  { Incercam sa creem noua coloana }
  if FAdaugare then
     with GetTmpADOQuery do
       try
          ParamCheck := False;
          Sql.Add('SELECT 1 FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''CULGEST_DOCUM'') AND NAME LIKE '+QuotedStr(edNumeColoana.Text));
          Open;
          if not IsEmpty then
             raise EContaHandledError.Create('Coloana '+QuotedStr(edNumeColoana.Text)+' exista deja in tabela de destinatie !');
       finally
          Free;
       end;
end;

procedure TfrmAdaugareColoana.SetDataSet(const Value: TDataSet);
begin
  FDataSet := Value;
end;

procedure TfrmAdaugareColoana.Button1Click(Sender: TObject);
begin
  if Trim(edNumeColoana.Text) = '' then
     raise EContaHandledError.Create('Trebuie sa specificati un nume pentru noul camp !');
  SaveData;
end;

procedure TfrmAdaugareColoana.FormCreate(Sender: TObject);
var
  lFieldType : TFieldType;
begin
{Curatam lista de valori}
  edtTipData.Values.Clear;
  edtTipData.Descriptions.Clear;
  edtClasaEditare.Values.Clear;
  edtClasaEditare.Descriptions.Clear;

{ Incarcam tipurile de campuri }
  for lFieldType := Low(TFieldType) to High(TFieldType) do
   if lFieldType <> ftUnknown then begin
      edtTipData.Values.Add(IntToStr(Integer(lFieldType)));
      edtTipData.Descriptions.Add(FieldTypeNames[lFieldType]);
   end;

{ Citim clasele asociate gridului si inspectorului }
  for lFieldType := Low(DefaultDBGridColumnType) to High(DefaultDBGridColumnType) do begin
    if edtClasaEditare.Values.IndexOf(DefaultDBGridColumnType[lFieldType].ColumnClass.ClassName) = -1 then begin
       edtClasaEditare.Values.Add(DefaultDBGridColumnType[lFieldType].ColumnClass.ClassName);
       edtClasaEditare.Descriptions.Add(DefaultDBGridColumnType[lFieldType].ColumnClass.ClassName);
    end;
  end;

end;

procedure TfrmAdaugareColoana.edtTipDataChange(Sender: TObject);
var lFieldType: TFieldType;
begin
  edtTipData.Tag := StrToInt(edtTipData.Text);
  lFieldType := TFieldType(edtTipData.Tag);
  if lFieldType in [ftString .. ftTypedBinary] then
      edtClasaEditare.Text := DefaultDBGridColumnType[lFieldType].ColumnClass.ClassName
  else
      edtClasaEditare.Text := 'TdxDBGridMaskColumn';
end;

end.
