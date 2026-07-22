unit fmAnexeBilantUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, StdCtrls, cxControls, DB, ZDataSet, ImgList, Menus, cxButtons,
  cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit, cxLookupEdit, cxDBLookupEdit,
  cxDBLookupComboBox, cxDBEdit, cxImageComboBox, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid, cxCheckBox, cxSpinEdit, cxTL,
  cxInplaceContainer, cxTLData, cxDBTL, cxGroupBox, cxRepartitorPanel, dxmdaset, cxLookAndFeelPainters,
  cxGraphics, cxDataStorage, ZAbstractRODataset, ZAbstractDataset, cxMemo, cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxSplitter,
  cxNavigator, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxDateRanges, dxScrollbarAnnotations ;


const
  WM_ANEXA_POST = WM_USER +1;

type

  THistory = class(TList);

  THistoryFormula = class(TObject)
  private
    FIdAnexeColoane: Integer;
    FIdAnexeRanduri: Integer;
    FIdAnexe: Integer;
    FTabCaption: String;
    FContList: String;
    FFieldName: String;
  public
    property IdAnexeRanduri: Integer read FIdAnexeRanduri write FIdAnexeRanduri;
    property IdAnexeColoane: Integer read FIdAnexeColoane write FIdAnexeColoane;
    property IdAnexe       : Integer read FIdAnexe        write FIdAnexe;
    property TabCaption    : String  read FTabCaption     write FTabCaption;
    property FieldName     : String  read FFieldName      write FFieldName;
    property ContList      : String  read FContList       write FContList;
  end;

  TfrmAnexeBilant = class(TForm)
    pnTop: TPanel;
    Panel2: TPanel;
    Panel3: TPanel;
    Panel4: TPanel;
    pnColoane: TGroupBox;
    Splitter2: TcxSplitter;
    pnRanduri: TGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Panel6: TPanel;
    Panel7: TPanel;
    qryRanduri: TZQuery;
    QryAnexe: TZQuery;
    DTAnexe: TDataSource;
    DTRanduri: TDataSource;
    DTColoane: TDataSource;
    QryColoane: TZQuery;
    DTFormula: TDataSource;
    qryFormula: TZQuery;
    tblDetaliiFormula: TdxMemData;
    tblDetaliiFormulaCONT: TStringField;
    tblDetaliiFormulaPARINTE: TStringField;
    tblDetaliiFormulaDESCRIERE: TStringField;
    tblDetaliiFormulaSIDA: TIntegerField;
    tblDetaliiFormulaSIDS: TIntegerField;
    tblDetaliiFormulaRTDA: TIntegerField;
    tblDetaliiFormulaRTDS: TIntegerField;
    tblDetaliiFormulaSFDA: TIntegerField;
    tblDetaliiFormulaSFDS: TIntegerField;
    tblDetaliiFormulaSICA: TIntegerField;
    tblDetaliiFormulaSICS: TIntegerField;
    tblDetaliiFormulaRTCA: TIntegerField;
    tblDetaliiFormulaRTCS: TIntegerField;
    tblDetaliiFormulaSFCA: TIntegerField;
    tblDetaliiFormulaSFCS: TIntegerField;
    dtDetaliiFormula: TDataSource;
    qryPlanConturi: TZQuery;
    selImages: TImageList;
    dtLstAnexe: TDataSource;
    qryLstAnexe: TZQuery;
    ppIstoric: TPopupMenu;
    PanelBottom1: TPanel;
    Panel8: TPanel;
    Splitter3: TcxSplitter;
    pnFormule: TGroupBox;
    Panel5: TPanel;
    Splitter4: TcxSplitter;
    Splitter5: TcxSplitter;
    Panel9: TPanel;
    Label6: TLabel;
    descFormula: TcxMemo;
    Panel10: TPanel;
    descNewFormula: TcxMemo;
    Label5: TLabel;
    edIdAnexa: TEdit;
    FormulaMenu: TPopupMenu;
    CmdCopyFormula: TMenuItem;
    Cmd_PasteFormula: TMenuItem;
    CmdEmptyFormula: TMenuItem;
    qryCopyPaste: TZQuery;
    btnAddColoana: TcxButton;
    btnDelColoana: TcxButton;
    btnSaveColoana: TcxButton;
    btnAddRand: TcxButton;
    btnDelRand: TcxButton;
    btnSaveRand: TcxButton;
    btnCancel: TcxButton;
    btnSalveaza: TcxButton;
    btnPreview: TcxButton;
    btnOk: TcxButton;
    btnDelAnexa: TcxButton;
    btnAddAnexa: TcxButton;
    edtAnexaLookup: TcxLookupComboBox;
    edtDenumire: TcxDBTextEdit;
    edtAnexa: TcxDBTextEdit;
    edtTitlu: TcxDBTextEdit;
    edtCuInchidere: TcxDBImageComboBox;
    edtTipAnexa: TcxDBImageComboBox;
    GridRanduriL: TcxGridLevel;
    cxGridRanduri: TcxGrid;
    GridRanduri: TcxGridDBTableView;
    GridRanduriID_ANEXE_RANDURI: TcxGridDBColumn;
    GridRanduriID_ANEXE_BILANT: TcxGridDBColumn;
    GridRanduriNR_CRT: TcxGridDBColumn;
    GridRanduriDENUMIRE: TcxGridDBColumn;
    GridRanduriNR_RAND: TcxGridDBColumn;
    GridRanduriPE_CONT: TcxGridDBColumn;
    GridRanduriCUMULARE: TcxGridDBColumn;
    GridRanduriPOZITIE: TcxGridDBColumn;
    GridRanduriEVAL_ORDER: TcxGridDBColumn;
    GridRandurisemn: TcxGridDBColumn;
    GridColoaneL: TcxGridLevel;
    cxGridColoane: TcxGrid;
    GridColoane: TcxGridDBTableView;
    GridColoaneID_ANEXE_COLOANE: TcxGridDBColumn;
    GridColoaneID_ANEXE_BILANT: TcxGridDBColumn;
    GridColoaneNR_ORDINE: TcxGridDBColumn;
    GridColoaneTIP_COLOANA: TcxGridDBColumn;
    GridColoaneCAPTURA: TcxGridDBColumn;
    GridColoaneTIP_FORMULA: TcxGridDBColumn;
    GridColoaneFORMULA_STANDARD: TcxGridDBColumn;
    TreeDetaliiFormula: TcxDBTreeList;
    TreeDetaliiFormulaRecId: TcxDBTreeListColumn;
    TreeDetaliiFormulaCONT: TcxDBTreeListColumn;
    TreeDetaliiFormulaPARINTE: TcxDBTreeListColumn;
    TreeDetaliiFormulaDESCRIERE: TcxDBTreeListColumn;
    TreeDetaliiFormulaSIDA: TcxDBTreeListColumn;
    TreeDetaliiFormulaSIDS: TcxDBTreeListColumn;
    TreeDetaliiFormulaRTDA: TcxDBTreeListColumn;
    TreeDetaliiFormulaRTDS: TcxDBTreeListColumn;
    TreeDetaliiFormulaSFDA: TcxDBTreeListColumn;
    TreeDetaliiFormulaSFDS: TcxDBTreeListColumn;
    TreeDetaliiFormulaSICA: TcxDBTreeListColumn;
    TreeDetaliiFormulaSICS: TcxDBTreeListColumn;
    TreeDetaliiFormulaRTCA: TcxDBTreeListColumn;
    TreeDetaliiFormulaRTCS: TcxDBTreeListColumn;
    TreeDetaliiFormulaSFCA: TcxDBTreeListColumn;
    TreeDetaliiFormulaSFCS: TcxDBTreeListColumn;
    TreeDetaliiFormulaDENUMIRE: TcxDBTreeListColumn;
    qryPlanBuget: TZQuery;
    chkFastEditing: TcxCheckBox;
    chkAutoAscundeColoane: TcxCheckBox;
    GridRanduricodExtern: TcxGridDBColumn;
    GridColoanecodExtern: TcxGridDBColumn;
    edtCodExtern: TcxDBTextEdit;
    Label7: TLabel;
    pnExecutie: TPanel;
    edtTipClasificatie: TcxImageComboBox;
    Label8: TLabel;
    edtClasificatie: TcxRepartitorPanel;
    TreeBugete: TcxDBTreeList;
    TreeBugeteDESCRIERE: TcxDBTreeListColumn;
    TreeBugeteDENUMIRE: TcxDBTreeListColumn;
    TreeBugeteCOD_BUGET: TcxDBTreeListColumn;
    tblDetaliiFormulaID: TStringField;
    lbTipClasificatie: TLabel;
    GridRanduribold: TcxGridDBColumn;
    Label9: TLabel;
    lbBalanta: TLabel;
    Label10: TLabel;
    edtRotunjire: TcxDBImageComboBox;
    edtNrZecimale: TcxDBSpinEdit;
    Cmd_Transform: TMenuItem;
    btnEditParams: TcxButton;
    btnParamsAnexa: TcxButton;
    btnCalcEvalOrder: TcxButton;
    btnCopiaza: TcxButton;
    qryPlanConturiExecutie: TZQuery;    
    TreeDetaliiFormulaFctCont: TcxDBTreeListColumn;
    tblDetaliiFormulaFCTCONT: TStringField;

    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnAddRandClick(Sender: TObject);
    procedure btnAddColoanaClick(Sender: TObject);
    procedure btnDelColoanaClick(Sender: TObject);
    procedure btnDelRandClick(Sender: TObject);
    procedure DTRanduriStateChange(Sender: TObject);
    procedure DTColoaneStateChange(Sender: TObject);
    procedure btnSaveRandClick(Sender: TObject);
    procedure btnSaveColoanaClick(Sender: TObject);
    procedure QryColoaneAfterOpen(DataSet: TDataSet);
    procedure qryRanduriAfterOpen(DataSet: TDataSet);
    procedure FormCreate(Sender: TObject);
    procedure qryFormulaNewRecord(DataSet: TDataSet);
    procedure FormDestroy(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure tblDetaliiFormulaNewRecord(DataSet: TDataSet);
    procedure descFormulaMouseMove(Sender: TObject; Shift: TShiftState; X,
      Y: Integer);
    procedure descFormulaClick(Sender: TObject);
    procedure tblDetaliiFormulaAfterPost(DataSet: TDataSet);
    procedure btnCancelClick(Sender: TObject);
    procedure btnSalveazaClick(Sender: TObject);
    procedure btnPreviewClick(Sender: TObject);
    procedure QryColoaneNewRecord(DataSet: TDataSet);
    procedure chkFastEditingClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure CmdCopyFormulaClick(Sender: TObject);
    procedure Cmd_PasteFormulaClick(Sender: TObject);
    procedure FormulaMenuPopup(Sender: TObject);
    procedure tblDetaliiFormulaAfterEdit(DataSet: TDataSet);
    procedure btnAddAnexaClick(Sender: TObject);
    procedure btnDelAnexaClick(Sender: TObject);
    procedure edtAnexaLookupPropertiesChange(Sender: TObject);
    procedure DTAnexeDataChange(Sender: TObject; Field: TField);
    procedure GridRanduriFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridColoaneFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure TreeDetaliiFormulaDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure edTipClasificatiePropertiesChange(Sender: TObject);
    procedure TreeBugeteDESCRIEREGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeBugeteDblClick(Sender: TObject);
    procedure TreeBugeteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtTipAnexaPropertiesChange(Sender: TObject);
    procedure edtClasificatiePopupInitPopup(Sender: TObject);
    procedure edtClasificatieValidate(Sender: TObject;
      var AKeyValue: Variant);
    procedure edtAnexaLookupPropertiesPopup(Sender: TObject);
    procedure edtRotunjirePropertiesChange(Sender: TObject);
    procedure btnEditParamsClick(Sender: TObject);
    procedure btnParamsAnexaClick(Sender: TObject);
    procedure btnCalcEvalOrderClick(Sender: TObject);
    procedure btnCopiazaClick(Sender: TObject);
  private
    FIdAnexeColoane : Integer;
    FIdAnexeRanduri : Integer;
    FIdAnexe        : Integer;
    FTipFormula     : Integer;
    FFormulaModified : Boolean;
    FTipClasificatie : Integer;    

    FHistory        : THistory;
//    FHistoryPos     : Integer;
    FPeCont: Boolean;
    IdAnexeRanduriCopy : Integer;
    IdAnexeColoaneCopy : Integer;



    function  GetOperand(var Symbol: String; var FieldName: String; var Sign: Char): Boolean;

    function ExtractContLeft(Symbol : String) : String;
    function ExtractContRight(Symbol : String) : String;    
    function ExtractTreeCont(Symbol : String) : String;


    procedure SetIdAnexeColoane(const Value: Integer);
    procedure SetIdAnexeRanduri(const Value: Integer);
    procedure UpdateQryFormule;
    procedure UpdateDescriereFormula(Memo: TcxMemo);
    procedure cxUpdateTopMost(Sender : TcxGridDBTableView);
    procedure SetIdAnexe(const Value: Integer);
    procedure DeleteDetaliiFormula(OnColumn: Boolean);

    procedure InitConturiList;


    procedure DoValidateTipFormula(Sender: TField);
    procedure SetTipFormula(const Value: Integer);
    procedure SetPeCont(const Value: Boolean);
    procedure SetFormulaModified(const Value: Boolean);
    procedure DoSaveFormula;

    procedure DoPostAnexa(var Message : TMessage); message WM_ANEXA_POST;
    procedure RefreshBugetDataSet;
    function GetDescriereForm(FieldName: String): String;
    function GetFieldNameForm(lSign, Formula: String): String;
    function ExtractClasaCont(Symbol: String): String;

    procedure LoadTranformMenu;
    procedure SubMClick(Sender : TObject);
    { Private declarations }
  protected
    procedure DoUndo;
    procedure DoRedo;
    procedure PushHistory;
    procedure SaveFormula;

  public
    { Public declarations }
    property IdAnexeRanduri: Integer read FIdAnexeRanduri write SetIdAnexeRanduri;
    property IdAnexeColoane: Integer read FIdAnexeColoane write SetIdAnexeColoane;
    property IdAnexe       : Integer read FIdAnexe        write SetIdAnexe;
    property TipFormula    : Integer read FTipFormula     write SetTipFormula;
    property PeCont        : Boolean read FPeCont         write SetPeCont;
    property FormulaModified: Boolean read FFormulaModified write SetFormulaModified;
  end;

implementation

{$R *.dfm}

uses
  infoCustomizeHook,
  ZeosDBUtile,
  DateUnit,
  StrUtils,
  frmPreviewAnexaUnit,
  CommonDBVar,
  AnexeParametrii,
  AnexeParametriiAlocare,
  AnexeCopy;

procedure TfrmAnexeBilant.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmAnexeBilant.btnAddRandClick(Sender: TObject);
begin
  QryRanduri.Append;
  QryRanduri.FieldByName('ID_ANEXE_RANDURI').AsInteger  := DBGetMaxValue('ANEXE_RANDURI', 'ID_ANEXE_RANDURI') + 1;
  QryRanduri.FieldByName('ID_ANEXE_BILANT').AsInteger   := QryRanduri.Params[0].Value;
  QryRanduri.FieldByName('PE_CONT').AsBoolean           := True;
  QryRanduri.Post;
  FIdAnexeRanduri := QryRanduri.FieldByName('ID_ANEXE_RANDURI').AsInteger;
end;

procedure TfrmAnexeBilant.btnAddColoanaClick(Sender: TObject);
begin
  QryColoane.Append;
  QryColoane.FieldByName('ID_ANEXE_BILANT').AsInteger  := QryColoane.Params[0].Value;
  QryColoane.FieldByName('TIP_FORMULA').AsInteger      := 1;
  QryColoane.Post;
  FIdAnexeColoane := QryColoane.FieldByName('ID_ANEXE_COLOANE').AsInteger;
end;

procedure TfrmAnexeBilant.btnDelColoanaClick(Sender: TObject);
begin
  if MessageDlg('Doriti stergerea coloanei '+QryColoane.FieldByName('CAPTURA').AsString+'?',
                mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
     DeleteDetaliiFormula(True);
     QryColoane.Delete;
  end;
end;

procedure TfrmAnexeBilant.btnDelRandClick(Sender: TObject);
begin
  if MessageDlg('Doriti stergerea randului '+QryRanduri.FieldByName('DENUMIRE').AsString+'?',
                mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
     DeleteDetaliiFormula(False);
     QryRanduri.Delete;
  end;
end;

procedure TfrmAnexeBilant.DTRanduriStateChange(Sender: TObject);
begin
  btnSaveRand.Enabled := QryRanduri.State in dsEditModes;
end;

procedure TfrmAnexeBilant.DTColoaneStateChange(Sender: TObject);
begin
  btnSaveColoana.Enabled := QryColoane.State in dsEditModes;
end;

procedure TfrmAnexeBilant.btnSaveRandClick(Sender: TObject);
begin
  if QryRanduri.State in dsEditModes then QryRanduri.Post;
end;

procedure TfrmAnexeBilant.btnSaveColoanaClick(Sender: TObject);
begin
  if QryColoane.State in dsEditModes then QryColoane.Post;
end;

procedure TfrmAnexeBilant.SetIdAnexeColoane(const Value: Integer);
begin
  FIdAnexeColoane := Value;
  UpdateQryFormule;
end;

procedure TfrmAnexeBilant.SetIdAnexeRanduri(const Value: Integer);
begin
  FIdAnexeRanduri := Value;
  UpdateQryFormule;
end;

procedure TfrmAnexeBilant.UpdateQryFormule;
begin
  qryFormula.Close;
  qryFormula.Params[0].Value := FIdAnexeRanduri;
  qryFormula.Params[1].Value := FIdAnexeColoane;
  qryFormula.Open;
  UpdateDescriereFormula(descFormula);
  descNewFormula.Text := descFormula.Text;
  FormulaModified := False;
end;

procedure TfrmAnexeBilant.UpdateDescriereFormula(Memo: TcxMemo);
var
  lConturiLipsa : TStringList;
  lDescriere : String;
  lSplitList : TStringList;
  lFieldName : String;
  I, J : Integer;
  lSign : Char;
  lForm : String;
  lValue: Integer;

  lDestField: TField;
  lSymbol     : String;
  lDescSymbol : String;
  lParent     : String;

  lColumn      : TcxDBTreeListColumn;

  lVisibleCols : TList;

begin
  lVisibleCols := TList.Create;
  TreeDetaliiFormula.BeginUpdate;
  tblDetaliiFormula.DisableControls;
  tblDetaliiFormula.BeforePost := nil;
  tblDetaliiFormula.AfterPost  := nil;
  try
    if Memo = descFormula then InitConturiList;
    Memo.Lines.Clear;
    lDescriere := '';
    lSplitList := TStringList.Create;
    lConturiLipsa := TStringList.Create;
    try
      lConturiLipsa.Sorted := True;
      lConturiLipsa.Duplicates := dupIgnore;
      lSplitList.Sorted := True;
      for J := 0 to qryFormula.FieldCount-1 do begin
        lFieldName := Trim(qryFormula.Fields[J].FieldName);
        if Length(lFieldName) = 4 then begin

          lDestField := tblDetaliiFormula.FindField(lFieldName);

          if not Assigned(lDestField) then
            raise EContaHandledError.Create('Eroare : Campul '+lFieldName+' nu exista in tabela de culegere !');

          if lFieldName[4] = 'A' then lSign := '+'
          else lSign := '-';
          lSplitList.CommaText := qryFormula.Fields[J].AsString;


          lForm := GetDescriereForm(lFieldName);
          if lForm = '<->' then Continue;

          if lSplitList.Count > 0 then begin
            lColumn := cxFindColumnByFieldName(TreeDetaliiFormula, lFieldName);
            if (Assigned(lColumn)) and (lVisibleCols.IndexOf(lColumn) = -1) then lVisibleCols.Add(lColumn);

            for I := 0 to lSplitList.Count-1 do begin
              lSymbol := Trim(lSplitList[I]);
              if lSymbol > '' then begin
                 lDescSymbol := lForm + '('+lSymbol+')';

                 if pos('**', lSymbol) > 0 then lValue := 4
                 else if pos('*', lSymbol) > 0 then lValue := 3
                 else lValue := 1;

                 lSymbol := StringReplace(lSymbol, '*', '', [rfReplaceAll, rfIgnoreCase]);

                 lSymbol := ExtractTreeCont(lSymbol);

                 if tblDetaliiFormula.Locate('CONT', lSymbol, []) then begin
                    tblDetaliiFormula.Edit;
                    lDestField.AsInteger := lValue;
                    tblDetaliiFormula.Post;
                    { Aplicam regulile si pe parinti }
                    lParent := tblDetaliiFormulaPARINTE.AsString;
                    while (tblDetaliiFormula.Locate('CONT', lParent, [])) and
                          (Trim(tblDetaliiFormulaCONT.AsString) <> Trim(tblDetaliiFormulaPARINTE.AsString)) do begin
                      { Daca avem un sintetic bifat si acum bifam copilul, il scoatem in lista de erori }
                      if lDestField.AsInteger = 1 then
                        { Adaugam Eroare La contul de sintetic, nu putem avea in formula si sintetic si analitic }
                        lConturiLipsa.AddObject(lParent, TObject(2))
                      else begin
                        tblDetaliiFormula.Edit;
                        lDestField.AsInteger := 2;
                        tblDetaliiFormula.Post;
                      end;
                      lParent := tblDetaliiFormulaPARINTE.AsString;
                    end;

                 end
                 else begin
                    lDescSymbol := '**'+lDescSymbol;
                    lConturiLipsa.AddObject(lSymbol, TObject(1));
                 end;

                 lDescriere := lDescriere + lSign + lDescSymbol;
              end;
            end;
          end;
        end;
      end;
      Memo.Lines.Add('-FORMULA : '+lDescriere);
      if lConturiLipsa.Count > 0 then begin
        lDescriere := '';
        for J := 0 to lConturiLipsa.Count-1 do
          if Integer(lConturiLipsa.Objects[J]) = 1 then begin
            if lDescriere > '' then lDescriere := lDescriere + ', ';
            lDescriere := lDescriere + ' ' + lConturiLipsa[J]
          end;
        if lDescriere > '' then begin
          lDescriere := '-SIMBOLURI LIPSA : '+lDescriere;
          Memo.Lines.Add(lDescriere);
        end;
        lDescriere := '';
        for J := 0 to lConturiLipsa.Count-1 do
          if Integer(lConturiLipsa.Objects[J]) = 2 then begin
            if lDescriere > '' then lDescriere := lDescriere + ', ';
            lDescriere := lDescriere + ' ' + lConturiLipsa[J]
          end;
        if lDescriere > '' then begin
          lDescriere := '-SINTETIC SI ANALITIC : '+lDescriere;
          Memo.Lines.Add(lDescriere);
        end;
      end;

      if chkAutoAscundeColoane.Checked then begin
        if lVisibleCols.Count > 0 then begin
          for J := 0 to TreeDetaliiFormula.ColumnCount-1 do begin
            lColumn := TcxDBTreeListColumn(TreeDetaliiFormula.Columns[J]);
            if lColumn.Position.BandIndex = 1 then
              lColumn.Visible := lVisibleCols.IndexOf(lColumn) > -1;
          end;
        end;
      end;
    finally
      lSplitList.Free;
      lConturiLipsa.Free;
    end;
  finally
    tblDetaliiFormula.AfterPost  := tblDetaliiFormulaAfterPost;
    tblDetaliiFormula.EnableControls;
    TreeDetaliiFormula.EndUpdate;
    lVisibleCols.Free;
  end;
end;

procedure TfrmAnexeBilant.QryColoaneAfterOpen(DataSet: TDataSet);
begin
  cxUpdateTopMost(GridColoane);
  QryColoane.FindField('TIP_FORMULA').OnValidate := DoValidateTipFormula;
  LoadTranformMenu;
end;


procedure TfrmAnexeBilant.qryRanduriAfterOpen(DataSet: TDataSet);
begin
  cxUpdateTopMost(GridRanduri);
end;

procedure TfrmAnexeBilant.SetIdAnexe(const Value: Integer);

  procedure OpenInner(DataSet: TZQuery);
   begin
    DataSet.Close;
    DataSet.Params[0].Value := Value;
    DataSet.Open;
   end;

begin
  FIdAnexe := Value;
  try
    qryAnexe.DisableControls;
    qryRanduri.DisableControls;
    qryColoane.DisableControls;
    OpenInner(qryAnexe);
    OpenInner(qryRanduri);
    OpenInner(qryColoane);
  finally
    qryAnexe.EnableControls;
    qryRanduri.EnableControls;
    qryColoane.EnableControls;
  end;
end;

procedure TfrmAnexeBilant.DeleteDetaliiFormula(OnColumn: Boolean);
var
  lSQL: String;
begin
  lSQL := 'delete from ANEXE_RANDURI_COLOANE where ';
    if not OnColumn then lSQL := lSQL + 'ID_ANEXE_RANDURI = '+IntToStr(FIdAnexeRanduri)
    else lSQL := lSQL + 'ID_ANEXE_COLOANE = '+IntToStr(FIdAnexeColoane);
  DBExecSQL(lSQL);
end;

procedure TfrmAnexeBilant.FormCreate(Sender: TObject);
begin
  if DBProcExists('spAnexeGetPlanCont') then
    qryPlanConturi.SQL.Text := 'exec spAnexeGetPlanCont';

//  if ExistSQLObject('spAnexeGetPlanBuget') = 1 then
//    qryPlanBuget.SQL.Text := 'exec spAnexeGetPlanBuget';

{$IFNDEF VER230}
  infoHook.UnHookedNameList.Add(TreeDetaliiFormula.Name);
  InfoHook.LoadControlSettings(Self, 'FormulareBilant', infoHook.FileName);
{$ENDIF}

  FHistory := THistory.Create    ;



  qryLstAnexe.Open;
  qryPlanConturi.Open;
  qryPlanConturiExecutie.Open;  
  RefreshBugetDataSet;
  
  WindowState := wsMaximized;
end;

procedure TfrmAnexeBilant.DoValidateTipFormula(Sender: TField);
begin
  TipFormula := Sender.AsInteger;
end;

procedure TfrmAnexeBilant.SetTipFormula(const Value: Integer);
begin
  FTipFormula := Value;
  if FTipFormula = 0 then DeleteDetaliiFormula(True)
  else begin
    if not DBRecordExists('ANEXE_RANDURI_COLOANE', 'WHERE ID_ANEXE_COLOANE = '+QryColoane.FieldByName('ID_ANEXE_COLOANE').AsString +
                    ' AND ID_ANEXE_RANDURI = '+IntToStr(FIdAnexeRanduri)) then begin
      qryFormula.Append;
      // Trebuie asa deoarece in cazul in care se adauga o noua coloana -> valoarea implicita nu este in stocata in proprietate
      // aici se ajunge pe onValidate-ul de pe campul TipFormula
      // si in acelasi timp fortam si modified -> True
      qryFormula.FieldByName('ID_ANEXE_COLOANE').AsInteger := QryColoane.FieldByName('ID_ANEXE_COLOANE').AsInteger;
      qryFormula.Post;
    end
    else UpdateQryFormule;
  end;
end;

procedure TfrmAnexeBilant.qryFormulaNewRecord(DataSet: TDataSet);
begin
  qryFormula.FieldByName('ID_ANEXE_RANDURI_COLOANE').AsInteger := DBGetMaxValue('ANEXE_RANDURI_COLOANE', 'ID_ANEXE_RANDURI_COLOANE') + 1;
  qryFormula.FieldByName('ID_ANEXE_RANDURI').AsInteger := FIdAnexeRanduri;
  qryFormula.FieldByName('ID_ANEXE_COLOANE').AsInteger := FIdAnexeColoane;
end;

procedure TfrmAnexeBilant.FormDestroy(Sender: TObject);
begin
  while FHistory.Count > 0 do
    TObject(FHistory[0]).Free;
  FHistory.Free;
  {$IFNDEF VER230}
  InfoHook.SaveControlSettings(Self, 'FormulareBilant', infoHook.FileName);
  {$ENDIF}
end;

procedure TfrmAnexeBilant.DoRedo;
begin
end;

procedure TfrmAnexeBilant.DoUndo;
begin

end;

procedure TfrmAnexeBilant.PushHistory;
var
  lItem     : TMenuItem;
  lHistPos  : THistoryFormula;
begin

  lHistPos := THistoryFormula.Create;

  lHistPos.IdAnexeRanduri := FIdAnexeRanduri;
  lHistPos.IdAnexeColoane := FIdAnexeColoane;
  lHistPos.IdAnexe        := FIdAnexe;

  FHistory.Add(lHistPos);

  lItem := TMenuItem.Create(ppIstoric);

  with lItem do begin
    Caption := lHistPos.TabCaption + ' : ' + lHistPos.FieldName;
    Hint    := descFormula.Lines.Text;
    Tag     := Integer(lHistPos);
  end;

  ppIstoric.Items.Add(lItem);
end;

procedure TfrmAnexeBilant.btnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmAnexeBilant.SaveFormula;
begin

end;

procedure TfrmAnexeBilant.SetPeCont(const Value: Boolean);
begin
  if Value <> FPeCont then begin
    FPeCont := Value;
    { Stergem tabela anterioara }

    if FPeCont then begin
      if (edtTipAnexa.EditValue=1) then begin
        TreeDetaliiFormulaSIDA.Visible := True;
        TreeDetaliiFormulaSIDS.Visible := True;
        TreeDetaliiFormulaRTDA.Visible := True;
        TreeDetaliiFormulaRTDS.Visible := True;
        TreeDetaliiFormulaSFDA.Visible := True;
        TreeDetaliiFormulaSFDS.Visible := True;
        TreeDetaliiFormulaSICA.Visible := True;
        TreeDetaliiFormulaSICS.Visible := True;
        TreeDetaliiFormulaRTCA.Visible := True;
        TreeDetaliiFormulaRTCS.Visible := True;
        TreeDetaliiFormulaSFCA.Visible := True;
        TreeDetaliiFormulaSFCS.Visible := True;

        TreeDetaliiFormulaDESCRIERE.Caption.Text := 'Executie Bugetara';
        TreeDetaliiFormulaSIDA.Caption.Text      := 'Cred.Init.+';
        TreeDetaliiFormulaSIDS.Caption.Text      := 'Cred.Init.-';
        TreeDetaliiFormulaRTDA.Caption.Text      := 'Cred.Trim.+';
        TreeDetaliiFormulaRTDS.Caption.Text      := 'Cred.Trim.-';
        TreeDetaliiFormulaSFDA.Caption.Text      := 'Ang.Bug.+';
        TreeDetaliiFormulaSFDS.Caption.Text      := 'Ang.Bug.-';
        TreeDetaliiFormulaSICA.Caption.Text      := 'Ang.Leg.+';
        TreeDetaliiFormulaSICS.Caption.Text      := 'Ang.Leg.-';
        TreeDetaliiFormulaRTCA.Caption.Text      := 'Plati.Ef+';
        TreeDetaliiFormulaRTCS.Caption.Text      := 'Plati.Ef-';
        TreeDetaliiFormulaSFCA.Caption.Text      := 'Chelt.EF+';
        TreeDetaliiFormulaSFCS.Caption.Text      := 'Chelt.EF-';

      end
      else if (edtTipAnexa.EditValue=2) then begin
        TreeDetaliiFormulaSIDA.Visible := True;
        TreeDetaliiFormulaSIDS.Visible := True;
        TreeDetaliiFormulaRTDA.Visible := True;
        TreeDetaliiFormulaRTDS.Visible := True;
        TreeDetaliiFormulaSFDA.Visible := True;
        TreeDetaliiFormulaSFDS.Visible := True;
        TreeDetaliiFormulaSICA.Visible := True;
        TreeDetaliiFormulaSICS.Visible := True;
        TreeDetaliiFormulaRTCA.Visible := True;
        TreeDetaliiFormulaRTCS.Visible := True;
        TreeDetaliiFormulaSFCA.Visible := True;
        TreeDetaliiFormulaSFCS.Visible := True;

        TreeDetaliiFormulaDESCRIERE.Caption.Text := 'Executie Bugetara';
        TreeDetaliiFormulaSIDA.Caption.Text      := 'Prev.Init+';
        TreeDetaliiFormulaSIDS.Caption.Text      := 'Prev.Init-';
        TreeDetaliiFormulaRTDA.Caption.Text      := 'Prev.Trim.+';
        TreeDetaliiFormulaRTDS.Caption.Text      := 'Prev.Trim.-';
        TreeDetaliiFormulaSFDA.Caption.Text      := 'Drep.precedente+';
        TreeDetaliiFormulaSFDS.Caption.Text      := 'Drep.precedente-';
        TreeDetaliiFormulaSICA.Caption.Text      := 'Drep.curente+';
        TreeDetaliiFormulaSICS.Caption.Text      := 'Drep.curente-';
        TreeDetaliiFormulaRTCA.Caption.Text      := 'Incasari.real+';
        TreeDetaliiFormulaRTCS.Caption.Text      := 'Incasari.real-';
        TreeDetaliiFormulaSFCA.Caption.Text      := 'Stingere.alt.+';
        TreeDetaliiFormulaSFCS.Caption.Text      := 'Stingere.alt.-';
      end
      else begin
        TreeDetaliiFormulaSIDA.Visible := True;
        TreeDetaliiFormulaSIDS.Visible := True;
        TreeDetaliiFormulaRTDA.Visible := True;
        TreeDetaliiFormulaRTDS.Visible := True;
        TreeDetaliiFormulaSFDA.Visible := True;
        TreeDetaliiFormulaSFDS.Visible := True;
        TreeDetaliiFormulaSICA.Visible := True;
        TreeDetaliiFormulaSICS.Visible := True;
        TreeDetaliiFormulaRTCA.Visible := True;
        TreeDetaliiFormulaRTCS.Visible := True;
        TreeDetaliiFormulaSFCA.Visible := True;
        TreeDetaliiFormulaSFCS.Visible := True;

        TreeDetaliiFormulaDESCRIERE.Caption.Text := 'Cont Contabil';
        TreeDetaliiFormulaSIDA.Caption.Text      := 'SID +';
        TreeDetaliiFormulaSIDS.Caption.Text      := 'SID -';
        TreeDetaliiFormulaRTDA.Caption.Text      := 'RTD +';
        TreeDetaliiFormulaRTDS.Caption.Text      := 'RTD -';
        TreeDetaliiFormulaSFDA.Caption.Text      := 'SFD +';
        TreeDetaliiFormulaSFDS.Caption.Text      := 'SFD -';
        TreeDetaliiFormulaSICA.Caption.Text      := 'SIC +';
        TreeDetaliiFormulaSICS.Caption.Text      := 'SIC -';
        TreeDetaliiFormulaRTCA.Caption.Text      := 'RTC +';
        TreeDetaliiFormulaRTCS.Caption.Text      := 'RTC -';
        TreeDetaliiFormulaSFCA.Caption.Text      := 'SFC +';
        TreeDetaliiFormulaSFCS.Caption.Text      := 'SFC -';
      end
    end
    else begin
      TreeDetaliiFormulaSIDA.Visible := True;
      TreeDetaliiFormulaSIDS.Visible := True;
      TreeDetaliiFormulaRTDA.Visible := False;
      TreeDetaliiFormulaRTDS.Visible := False;
      TreeDetaliiFormulaSFDA.Visible := False;
      TreeDetaliiFormulaSFDS.Visible := False;
      TreeDetaliiFormulaSICA.Visible := False;
      TreeDetaliiFormulaSICS.Visible := False;
      TreeDetaliiFormulaRTCA.Visible := False;
      TreeDetaliiFormulaRTCS.Visible := False;
      TreeDetaliiFormulaSFCA.Visible := False;
      TreeDetaliiFormulaSFCS.Visible := False;

      TreeDetaliiFormulaDESCRIERE.Caption.Text := 'Rand Anexa';
      TreeDetaliiFormulaSIDA.Caption.Text      := 'RAND +';
      TreeDetaliiFormulaSIDS.Caption.Text      := 'RAND -';
    end;

    TreeDetaliiFormulaRTDA.Options.Editing := FPeCont;
    TreeDetaliiFormulaRTDS.Options.Editing := FPeCont;
    TreeDetaliiFormulaSFDA.Options.Editing := FPeCont;
    TreeDetaliiFormulaSFDS.Options.Editing := FPeCont;
    TreeDetaliiFormulaSICA.Options.Editing := FPeCont;
    TreeDetaliiFormulaSICS.Options.Editing := FPeCont;
    TreeDetaliiFormulaRTCA.Options.Editing := FPeCont;
    TreeDetaliiFormulaRTCS.Options.Editing := FPeCont;
    TreeDetaliiFormulaSFCA.Options.Editing := FPeCont;
    TreeDetaliiFormulaSFCS.Options.Editing := FPeCont;

  end;
end;

procedure TfrmAnexeBilant.InitConturiList;
var
  I: Integer;
begin
  tblDetaliiFormula.Active := False;
  tblDetaliiFormula.Active := True;

  if PeCont then begin
    if (edtTipAnexa.EditValue = 1) or (edtTipAnexa.EditValue = 2) then tblDetaliiFormula.LoadFromDataSet(qryPlanBuget)
                                  else if edtTipAnexa.EditValue = 3 then tblDetaliiFormula.LoadFromDataSet(qryPlanConturiExecutie)    
                                  else tblDetaliiFormula.LoadFromDataSet(qryPlanConturi)
  end
  else begin
    for I := 0 to GridRanduri.ViewData.RecordCount -1 do begin
      tblDetaliiFormula.Append;
      tblDetaliiFormulaID.AsString      := GridRanduri.ViewData.Records[I].DisplayTexts[GridRanduriNR_RAND.Index];      
      tblDetaliiFormulaCONT.AsString      := GridRanduri.ViewData.Records[I].DisplayTexts[GridRanduriNR_RAND.Index];
      tblDetaliiFormulaDESCRIERE.AsString := GridRanduri.ViewData.Records[I].DisplayTexts[GridRanduriDENUMIRE.Index];
      tblDetaliiFormula.Post;
    end;
  end;

end;

procedure TfrmAnexeBilant.tblDetaliiFormulaNewRecord(DataSet: TDataSet);
begin
  tblDetaliiFormulaSIDA.AsInteger := 0;
  tblDetaliiFormulaSIDS.AsInteger := 0;
  tblDetaliiFormulaRTDA.AsInteger := 0;
  tblDetaliiFormulaRTDS.AsInteger := 0;
  tblDetaliiFormulaSFDA.AsInteger := 0;
  tblDetaliiFormulaSFDS.AsInteger := 0;
  tblDetaliiFormulaSICA.AsInteger := 0;
  tblDetaliiFormulaSICS.AsInteger := 0;
  tblDetaliiFormulaRTCA.AsInteger := 0;
  tblDetaliiFormulaRTCS.AsInteger := 0;
  tblDetaliiFormulaSFCA.AsInteger := 0;
  tblDetaliiFormulaSFCS.AsInteger := 0;
end;

procedure TfrmAnexeBilant.descFormulaMouseMove(Sender: TObject;
  Shift: TShiftState; X, Y: Integer);
var
  lFieldName : String;
  lSymbol    : String;
  lHintDesc  : String;
  lSign      : Char;
  lPos       : TPoint;
  lSold      : Char;
  lTipCont   : Char;
  lWhen      : Char;
begin
  if GetOperand(lSymbol, lFieldName, lSign) then begin
    lFieldName := StringReplace(lFieldName, '*', '', [rfReplaceAll, rfIgnoreCase]);
    if lSign = '+' then lHintDesc := 'Adunare '#13#10
    else lHintDesc := 'Scadere '#13#10;
    if pos('RAND', lFieldName) = 1 then lHintDesc := lHintDesc + 'Randul : '+lSymbol
    else begin
      if Length(lFieldName) > 2 then begin
        lSold    := lFieldName[1];
        lWhen    := lFieldName[2];
        lTipCont := lFieldName[3];
        case lSold of
          'S' : lHintDesc := lHintDesc + 'Sold';
          'T' : lHintDesc := lHintDesc + 'Total Sume';
          else lHintDesc := lHintDesc + lSold;
        end;

        case lTipCont of
          'D' : lHintDesc := lHintDesc + ' Debitor';
          'C' : lHintDesc := lHintDesc + ' Creditor';
          else lHintDesc := lHintDesc + lTipCont;
        end;

        case lWhen of
          'I' : lHintDesc := lHintDesc + ' Initial';
          'F' : lHintDesc := lHintDesc + ' Final';
          else lHintDesc := lHintDesc + lWhen;
        end;
      end
      else lHintDesc := lHintDesc + lFieldName;
      lHintDesc := lHintDesc + #13#10 + 'Al contului : '+lSymbol;
    end;
    lPos := Point(X, Y);
    lPos := descFormula.ClientToScreen(lPos);
    descFormula.Hint := lHintDesc;
    Application.ActivateHint(lPos);
  end
  else descFormula.Hint := '';
end;

function TfrmAnexeBilant.GetOperand(var Symbol: String; var FieldName: String;
  var Sign: Char): Boolean;
var
  lMousePoint : TPoint;
  lMemoPos    : LRESULT;
  lCharIndex  : Word;
  lMemoText   : String;
  I, J        : Integer;
  lChunkPart  : String;
  
  lPos   : TPoint;
  lX,
  lY     : Integer;
  lLine  : String;
  lChunk : String;

begin
  Symbol    := '';
  FieldName := '';
  Sign      := #32;
  lMousePoint := descFormula.ScreenToClient(Mouse.CursorPos);
  lMemoPos    := descFormula.Perform(EM_CHARFROMPOS, 0, MakeLong(lMousePoint.X, lMousePoint.Y));
  lCharIndex  := LoWord(lMemoPos);
  Result := (lCharIndex > -1);
  if Result then begin
    lMemoText := descFormula.Text;
    { luam in stanga pana la semnul +/- sau inceput si in dreapta pana la semnul +/- sau sfarsit }
    I := lCharIndex;
    lChunkPart := '';
    while (I > 0) and not CharInSet(lMemoText[I], ['+', '-']) do
      lChunkPart := lMemoText[I] + lChunkPart;
    if (I > 0) and CharInSet(lMemoText[I], ['+', '-']) then
      lChunkPart := lMemoText[I] + lChunkPart;
    I := lCharIndex + 1;
    while (I <= Length(lMemoText)) and not CharInSet(lMemoText[I], ['+', '-']) do
      lChunkPart := lChunkPart + lMemoText[I];
    I := Pos('(', lChunkPart);
    Result := (I > 0) and (PosEx(')', lChunkPart, I+1) > 0);
    if Result then begin
      if CharInSet(lChunkPart[1], ['+', '-']) then begin
        Sign := lChunkPart[1];
        System.Delete(lChunkPart, 1, 1);
        Dec(I);
      end
      else
        Sign := #32;
      FieldName := Copy(lChunkPart, 1, I-1);
      Symbol    := Copy(lChunkPart, I, J - I);
    end;
  end;
end;

procedure TfrmAnexeBilant.descFormulaClick(Sender: TObject);
var
  lFieldName : String;
  lSymbol    : String;
  lSign      : Char;
  lField     : TField;
  lClasa     : String;
  lEddValue : TcxEditValue;
  lErrText : TCaption;
  lErr : Boolean;
begin
  if GetOperand(lSymbol, lFieldName, lSign) then begin
    lFieldName := StringReplace(lFieldName, '*', '', [rfReplaceAll, rfIgnoreCase]);
    lFieldName := GetFieldNameForm(lSign, lFieldName);

    lField := tblDetaliiFormula.FindField(lFieldName);
    lSymbol := StringReplace(lSymbol, '*', '', [rfReplaceAll, rfIgnoreCase]);

    lClasa  := ExtractClasaCont(lSymbol);
    lSymbol := ExtractTreeCont(lSymbol);

    if pnExecutie.Visible and (Trim(edtClasificatie.EditInput.Text) ='') then begin
        edtClasificatie.EditInput.Text := lClasa;
        lEddValue := lClasa;
        lErrText := 'Eroare';
        if Assigned( edtClasificatie.EditInput.Properties.OnValidate) then
           edtClasificatie.EditInput.Properties.OnValidate(nil, lEddValue, lErrText, lErr);
    end;

    tblDetaliiFormula.Locate('CONT', lSymbol, []);
    if Assigned(lField) then
       if cxFindColumnByFieldName(TreeDetaliiFormula, lField.FieldName) <> nil then
         TreeDetaliiFormula.FocusedColumn := cxFindColumnByFieldName(TreeDetaliiFormula, lField.FieldName);
  end;
end;

procedure TfrmAnexeBilant.tblDetaliiFormulaAfterPost(DataSet: TDataSet);
begin
  FormulaModified := True;
end;

procedure TfrmAnexeBilant.SetFormulaModified(const Value: Boolean);
begin
  FFormulaModified := Value;
  btnCancel.Enabled := Value;
  btnSalveaza.Enabled := Value;
end;

procedure TfrmAnexeBilant.btnCancelClick(Sender: TObject);
begin
  if MessageDlg('Doriti Abandonul modificarilor facute?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    UpdateQryFormule;
end;

procedure TfrmAnexeBilant.DoSaveFormula;
var
  SIDA,
  SIDS,
  RTDA,
  RTDS,
  SFDA,
  SFDS,
  SICA,
  SICS,
  RTCA,
  RTCS,
  SFCA,
  SFCS: String;

  lCodFunctional, lCodEconomic : String;

  procedure SavePosition(var Where: String; FieldName: String);
  var
    lValue: Integer;
  begin
    lValue := tblDetaliiFormula.FieldByName(FieldName).AsInteger;
    if (lValue > 0) and (lValue <> 2) then begin
      if Where > '' then Where := Where + ',';
      Where :=  Where + lCodFunctional + tblDetaliiFormulaCONT.AsString + lCodEconomic;
      if lValue = 3 then Where := Where + '*'
      else if lValue = 4 then Where := Where + '**';
    end;
  end;

  procedure SaveValue(const ValueList: String; FieldName: String);
   begin
    if not (qryFormula.State in dsEditModes) then
      qryFormula.Edit;
    if Trim(ValueList) > '' then
       qryFormula.FieldByName(FieldName).AsString := ValueList
    else
       qryFormula.FieldByName(FieldName).Clear;
   end;

var
  CurentRecNo: Integer;
begin
  lCodFunctional := '';
  lCodEconomic := '';
  if FPeCont and ((edtTipAnexa.EditValue = 1) or (edtTipAnexa.EditValue = 2)) then begin
    if Trim(edtClasificatie.EditInput.Text) = '' then begin
      MessageDlg('Trebuie completat ' + lbTipClasificatie.Caption + ' !' , mtError, [mbOK], 0);
      edtClasificatie.EditInput.SetFocus;
      Abort;
    end;
    if edtTipClasificatie.EditValue = 1 then lCodFunctional := Trim(edtClasificatie.EditInput.Text) + '|'
                                         else lCodEconomic   := '|'+Trim(edtClasificatie.EditInput.Text);
  end;

  { Salvam formula curenta }
//  qryformula.Connection.BeginTrans;

  TreeDetaliiFormula.BeginUpdate;
  tblDetaliiFormula.DisableControls;
  try
    CurentRecNo := tblDetaliiFormula.RecNo;
    tblDetaliiFormula.First;
    while not tblDetaliiFormula.Eof do begin
      SavePosition(SIDA, 'SIDA');
      SavePosition(SIDS, 'SIDS');
      SavePosition(RTDA, 'RTDA');
      SavePosition(RTDS, 'RTDS');
      SavePosition(SFDA, 'SFDA');
      SavePosition(SFDS, 'SFDS');
      SavePosition(SICA, 'SICA');
      SavePosition(SICS, 'SICS');
      SavePosition(RTCA, 'RTCA');
      SavePosition(RTCS, 'RTCS');
      SavePosition(SFCA, 'SFCA');
      SavePosition(SFCS, 'SFCS');
      tblDetaliiFormula.Next;
    end;
    SaveValue(SIDA, 'SIDA');
    SaveValue(SIDS, 'SIDS');
    SaveValue(RTDA, 'RTDA');
    SaveValue(RTDS, 'RTDS');
    SaveValue(SFDA, 'SFDA');
    SaveValue(SFDS, 'SFDS');
    SaveValue(SICA, 'SICA');
    SaveValue(SICS, 'SICS');
    SaveValue(RTCA, 'RTCA');
    SaveValue(RTCS, 'RTCS');
    SaveValue(SFCA, 'SFCA');
    SaveValue(SFCS, 'SFCS');
    if qryFormula.State in dsEditModes then qryFormula.Post;
    tblDetaliiFormula.RecNo := CurentRecNo;
  finally
    tblDetaliiFormula.EnableControls;
    TreeDetaliiFormula.EndUpdate;
  end;
end;

procedure TfrmAnexeBilant.btnSalveazaClick(Sender: TObject);
begin
  if tblDetaliiFormula.State in [dsEdit, dsInsert] then
    tblDetaliiFormula.Post; 
  DoSaveFormula;
  UpdateDescriereFormula(descFormula);
  FormulaModified := False;  
  descNewFormula.Text := descFormula.Text;
end;

procedure TfrmAnexeBilant.btnPreviewClick(Sender: TObject);
begin
  if FFormulaModified then begin
    if MessageDlg('Pentru previzualizare modificarile trebuie scrise in baza de date !'#13#10'Doriti scrierea lor si previzualizarea?',
                  mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
      btnSalveaza.Click;
      PreviewAnexa(Self.IdAnexe, edtTitlu.Text);
    end
  end
  else PreviewAnexa(Self.IdAnexe, edtTitlu.Text);
end;

procedure TfrmAnexeBilant.QryColoaneNewRecord(DataSet: TDataSet);
begin
  QryColoane.FieldByName('ID_ANEXE_COLOANE').AsInteger := DBGetMaxValue('ANEXE_COLOANE', 'ID_ANEXE_COLOANE') + 1;
end;

procedure TfrmAnexeBilant.chkFastEditingClick(Sender: TObject);
begin
  if chkFastEditing.Checked then begin
    TreeDetaliiFormula.OptionsBehavior.ImmediateEditor := True;
    GridColoane.OptionsBehavior.ImmediateEditor := True;
    GridRanduri.OptionsBehavior.ImmediateEditor := True;
  end
  else begin
    TreeDetaliiFormula.OptionsBehavior.ImmediateEditor := False;
    GridColoane.OptionsBehavior.ImmediateEditor := False;
    GridRanduri.OptionsBehavior.ImmediateEditor := False;
  end;
end;

procedure TfrmAnexeBilant.FormShow(Sender: TObject);
begin
  chkFastEditingClick(chkFastEditing);
end;

procedure TfrmAnexeBilant.CmdCopyFormulaClick(Sender: TObject);
begin
    IdAnexeRanduriCopy := IdAnexeRanduri;
    IdAnexeColoaneCopy := IdAnexeColoane;
end;

procedure TfrmAnexeBilant.Cmd_PasteFormulaClick(Sender: TObject);
var
  IdAnexeRanduriPaste : Integer;
  IdAnexeColoanePaste : Integer;
begin
   IdAnexeRanduriPaste := IdAnexeRanduri;
   IdAnexeColoanePaste := IdAnexeColoane;
   if (IdAnexeRanduriCopy <> -1) and (IdAnexeColoaneCopy <> -1) and
    (IdAnexeRanduriPaste <> -1) and (IdAnexeColoanePaste <> -1) then begin
    with qryCopyPaste do
        try
          Params.ParamByName('ID_RAND_COPY').Value := IdAnexeRanduriCopy;
          Params.ParamByName('ID_COL_COPY').Value := IdAnexeColoaneCopy;
          Params.ParamByName('ID_RAND_PASTE').Value := IdAnexeRanduriPaste;
          Params.ParamByName('ID_COL_PASTE').Value := IdAnexeColoanePaste;
          ExecSQL;
        except
        end;
    end;
    UpdateQryFormule;
end;

procedure TfrmAnexeBilant.FormulaMenuPopup(Sender: TObject);
begin
  Cmd_PasteFormula.Enabled := ((IdAnexeRanduriCopy <> -1 )and (IdAnexeColoaneCopy <> -1));
end;

procedure TfrmAnexeBilant.tblDetaliiFormulaAfterEdit(DataSet: TDataSet);
begin
  FormulaModified := True;
end;

procedure TfrmAnexeBilant.btnAddAnexaClick(Sender: TObject);
var
  lIdAnexe : Integer;
begin
  lIdAnexe := DBGetScallar('select isnull(max(id_anexe_bilant),0) + 1 from anexe_bilant');
  if not QryAnexe.Active then QryAnexe.Open;
  QryAnexe.Append;
  QryAnexe.FieldByName('DENUMIRE').AsString := '<Anexa Noua>';
  QryAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger := lIdAnexe;
  QryAnexe.FieldByName('STARE').AsBoolean := True;
  QryAnexe.FieldByName('tipRotunjire').AsInteger := 0;
  QryAnexe.FieldByName('nr_zecimale').AsInteger := 0;
  QryAnexe.Post;
  DBRefresh(qryLstAnexe);
  qryLstAnexe.Locate('ID_ANEXE_BILANT', lIdAnexe, []);
  edtAnexaLookup.EditValue := lIdAnexe;
  edtAnexaLookupPropertiesChange(nil);
end;

procedure TfrmAnexeBilant.btnDelAnexaClick(Sender: TObject);
begin
  if not QryAnexe.Active then Exit;
  if (MessageDlg(Format('Doriti stergerea anexei cu nr %s ?', [QryAnexe.FieldByName('ID_ANEXE_BILANT').AsString]), mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
    DBExecSQL('exec spAnexeDeleteAnexa ' + QryAnexe.FieldByName('ID_ANEXE_BILANT').AsString);
    //QryAnexe.Delete;
    DBRefresh(qryLstAnexe);
    qryLstAnexe.Last;
    edtAnexaLookup.EditValue := qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
    edtAnexaLookupPropertiesChange(nil);
  end;
end;

procedure TfrmAnexeBilant.edtAnexaLookupPropertiesChange(Sender: TObject);
begin
  IdAnexe := GetInteger(edtAnexaLookup.EditValue);
  edIdAnexa.Text := IntToStr(IdAnexe);
end;

procedure TfrmAnexeBilant.DoPostAnexa(var Message: TMessage);
begin
  if QryAnexe.State in [dsEdit, dsInsert] then QryAnexe.Post;
end;

procedure TfrmAnexeBilant.DTAnexeDataChange(Sender: TObject; Field: TField);
begin
  PostMessage(Handle, WM_ANEXA_POST, 0, 0);
end;

procedure TfrmAnexeBilant.GridRanduriFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then begin
    PeCont :=  GetBoolean(AFocusedRecord, GridRanduriPE_CONT.Index);
    if (not VarIsEmpty(AFocusedRecord.Values[GridRanduriID_ANEXE_RANDURI.Index]) ) and
       (not VarIsNull(AFocusedRecord.Values[GridRanduriID_ANEXE_RANDURI.Index]) ) then
       IdAnexeRanduri := AFocusedRecord.Values[GridRanduriID_ANEXE_RANDURI.Index];
  end
  else IdAnexeRanduri := -1;
end;

procedure TfrmAnexeBilant.cxUpdateTopMost(Sender: TcxGridDBTableView);
begin
  if Assigned(Sender.OnFocusedRecordChanged) then
    if Assigned(Sender.Controller.FocusedRecord) and Sender.Controller.FocusedRecord.IsData then
      Sender.OnFocusedRecordChanged(Sender, nil, Sender.Controller.FocusedRecord, False)
    else
      if Sender.ViewData.RecordCount > 0 then Sender.OnFocusedRecordChanged(Sender, nil, Sender.ViewData.Records[0], False);
end;

procedure TfrmAnexeBilant.GridColoaneFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var I : Integer;
begin
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then
    IdAnexeColoane := AFocusedRecord.Values[GridColoaneID_ANEXE_COLOANE.Index]
  else IdAnexeColoane := -1;
  for I := 0 to Cmd_Transform.Count - 1 do
    Cmd_Transform.Items[I].Visible := not (Cmd_Transform.Items[I].Tag = IdAnexeColoane);  
end;

procedure TfrmAnexeBilant.TreeDetaliiFormulaDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[TreeDetaliiFormulaCONT.ItemIndex]+' : '+ANode.Texts[TreeDetaliiFormulaDENUMIRE.ItemIndex];
end;

procedure TfrmAnexeBilant.edTipClasificatiePropertiesChange(
  Sender: TObject);
begin
  FTipClasificatie := StrToInt(edtTipClasificatie.EditValue);
  case FTipClasificatie of
    0: begin
         { Schimbam lista asociata }
         edtClasificatie.DataSource := FrmData.DTBGPlanEconomic;
         TreeBugete.DataController.DataSource := FrmData.DTBGPlanEconomic;
         TreeBugete.DataController.KeyField   := 'ID_BG_PLAN_ECONOMIC';
         lbTipClasificatie.Caption := 'Cod Ec.   :';
       end;
    1: begin
         { Schimbam lista asociata }
         edtClasificatie.DataSource := FrmData.DTBGPlanFunctional;
         TreeBugete.DataController.DataSource   := FrmData.DTBGPlanFunctional;
         TreeBugete.DataController.KeyField     := 'ID_BG_PLAN_FUNCTIONAL';
         lbTipClasificatie.Caption := 'Cod Funct. : ';
       end;
  end;
  RefreshBugetDataSet;
end;

procedure TfrmAnexeBilant.TreeBugeteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[TreeBugeteCOD_BUGET.ItemIndex] + ': '+ANode.Texts[TreeBugeteDENUMIRE.ItemIndex];
end;

procedure TfrmAnexeBilant.TreeBugeteDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmAnexeBilant.TreeBugeteKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreeBugeteDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmAnexeBilant.edtTipAnexaPropertiesChange(Sender: TObject);
begin
  pnExecutie.Visible     := (QryAnexe.FieldByName('tip_anexa').AsInteger >= 1) and (QryAnexe.FieldByName('tip_anexa').AsInteger <> 3);
  lbBalanta.Visible      := not (pnExecutie.Visible);
  edtCuInchidere.Visible := not (pnExecutie.Visible);
  RefreshBugetDataSet;  
end;

procedure TfrmAnexeBilant.edtClasificatiePopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmAnexeBilant.edtClasificatieValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  RefreshBugetDataSet;
  FormulaModified := True;
end;

procedure TfrmAnexeBilant.RefreshBugetDataSet;
var lBeforeOpen : Boolean;
begin
  lBeforeOpen := FFormulaModified;
  if qryPlanBuget.Active then qryPlanBuget.Close;
  qryPlanBuget.Params.ParamByName('codBuget').Value := edtClasificatie.EditInput.Text;
  qryPlanBuget.Params.ParamByName('peFunctional').Value := edtTipClasificatie.EditValue;
  qryPlanBuget.Params.ParamByName('numaiPlanificat').Value := False;
  if edtTipAnexa.EditValue = 2 then
    qryPlanBuget.Params.ParamByName('tipCV').Value := 1
  else
    qryPlanBuget.Params.ParamByName('tipCV').Value := 0;
  qryPlanBuget.Open;
  InitConturiList;
  UpdateDescriereFormula(descFormula);
  FormulaModified := lBeforeOpen;
end;

function TfrmAnexeBilant.ExtractClasaCont(Symbol: String): String;
begin
  Result := Symbol;
  if FPeCont and ((edtTipAnexa.EditValue = 1) or (edtTipAnexa.EditValue = 2)) then begin
    if edtTipClasificatie.EditValue = 1 then Result := ExtractContLeft(Symbol)
                                        else Result := ExtractContRight(Symbol);
  end;
end;

function TfrmAnexeBilant.ExtractTreeCont(Symbol: String): String;
begin
  Result := Symbol;
  if FPeCont and ((edtTipAnexa.EditValue = 1) or (edtTipAnexa.EditValue = 2)) then begin
    if edtTipClasificatie.EditValue = 1 then Result := ExtractContRight(Symbol)
                                        else Result := ExtractContLeft(Symbol);
  end;
end;

function TfrmAnexeBilant.ExtractContLeft(Symbol: String): String;
begin
  if pos('|', Symbol) = 0 then Symbol := Symbol + '|';
  Result := Copy(Symbol, 1, pos('|', Symbol) - 1);
end;

function TfrmAnexeBilant.ExtractContRight(Symbol: String): String;
begin
  if pos('|', Symbol) = 0 then Symbol := '|' + Symbol;
  Result := Copy(Symbol, pos('|', Symbol)+ 1, length(Symbol) -  pos('|', Symbol));
end;


function TfrmAnexeBilant.GetDescriereForm(FieldName : String ) : String;
begin
  Result := '<->';
  if not FPeCont then
    if (SameText(FieldName, 'SIDA')) or
       (SameText(FieldName, 'SIDS')) then Result := 'RAND'
    else Result := '<->'
  else begin
     Result := Copy(FieldName, 1, 3);
     FieldName  := Result;
     if (edtTipAnexa.EditValue=1) then begin
            if FieldName = 'SID' then Result := 'CredInit'
       else if FieldName = 'RTD' then Result := 'CredTrim'
       else if FieldName = 'SFD' then Result := 'AngBug'
       else if FieldName = 'SIC' then Result := 'AngLeg'
       else if FieldName = 'RTC' then Result := 'Plati'
       else if FieldName = 'SFC' then Result := 'Chelt';
     end
     else if (edtTipAnexa.EditValue=2) then begin
            if FieldName = 'SID' then Result := 'PrevInit'
       else if FieldName = 'RTD' then Result := 'PrevTrim'
       else if FieldName = 'SFD' then Result := 'DatPrec'
       else if FieldName = 'SIC' then Result := 'DatCurent'
       else if FieldName = 'RTC' then Result := 'Incasari'
       else if FieldName = 'SFC' then Result := 'AlteSting';
     end;
  end;
end;


function TfrmAnexeBilant.GetFieldNameForm(lSign, Formula: String): String;
begin
   Result := '';
   Result := Formula;
        if (Result = 'CredInit') or (Result = 'PrevInit') then Result := 'SID'
   else if (Result = 'CredTrim') or (Result = 'PrevTrim') then Result := 'RTD'
   else if (Result = 'AngBug') or (Result = 'DatPrec') then Result := 'SFD'
   else if (Result = 'AngLeg') or (Result = 'DatCurent') then Result := 'SIC'
   else if (Result = 'Plati') or (Result = 'Incasari') then Result := 'RTC'
   else if (Result = 'Chelt') or (Result = 'AlteSting') then Result := 'SFC';

    if lSign = '+' then Result := Result + 'A'
    else Result := Result + 'S';

    if Result = 'RANDS' then Result := 'SIDS'
    else if Result = 'RANDA' then Result := 'SIDA';
end;

procedure TfrmAnexeBilant.edtAnexaLookupPropertiesPopup(Sender: TObject);
begin
  DBRefresh(qryLstAnexe);
end;

procedure TfrmAnexeBilant.edtRotunjirePropertiesChange(Sender: TObject);
begin
  edtNrZecimale.Visible := (edtRotunjire.ItemIndex = 0);
end;

procedure TfrmAnexeBilant.LoadTranformMenu;
var
  SubM : TMenuItem;
  BookM : TBookMark;
begin
  Cmd_Transform.Clear;
  with QryColoane do
    try
      BookM := QryColoane.GetBookmark;
      First;
      while not eof do begin
        if FieldByName('TIP_FORMULA').AsInteger = 1 then begin
          SubM := TMenuItem.Create(Cmd_Transform);
          SubM.Caption := FieldByName('CAPTURA').AsString;
          SubM.Tag := FieldByName('ID_ANEXE_COLOANE').AsInteger;
          SubM.OnClick := SubMClick;
          Cmd_Transform.Add(SubM);
        end;
        Next;
      end;
    finally
      GotoBookmark(BookM);
      FreeBookmark(BookM);
    end;
end;

procedure TfrmAnexeBilant.SubMClick(Sender: TObject);
var
  idCol : Integer;
begin
  if not (Sender is TMenuItem) then Exit;
  idCol := TMenuItem(Sender).Tag;
  DBExecSQLFmt('exec [spAnexeTransform] %d, %d, %d', [idCol, idAnexeColoane, idAnexeRanduri] );
  UpdateQryFormule;
end;

procedure TfrmAnexeBilant.btnEditParamsClick(Sender: TObject);
begin
  with TfrmIntretinAnexeParametrii.Create(Application) do
  try
    //FIdAnexaBilant := qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
    ShowModal;
  finally
    Free;
  end;
end;

procedure TfrmAnexeBilant.btnParamsAnexaClick(Sender: TObject);
begin
  with TfrmAsocParam.Create(Application) do
  try
    IdAnexeBilant := qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
    ShowModal;
  finally
    Free;
  end;
end;

procedure TfrmAnexeBilant.btnCalcEvalOrderClick(Sender: TObject);
var S: String;
begin
  if not QryColoane.Locate('ID_ANEXE_COLOANE', FIdAnexeColoane, []) then Exit;
  S := QryColoane.FieldByName('CAPTURA').AsString;
  if (MessageDlg(Format('Doriti calcularea automata a ordinii de evaluare dupa coloana : %s', [S]), mtConfirmation, [mbYes, mbNo], 0) = mrNo)
    then Abort;
  DBExecSQLFmt('exec [spAnexeEvalOrder] %d, %d', [FIdAnexe, FIdAnexeColoane]);
  IdAnexe :=  FIdAnexe;
end;

procedure TfrmAnexeBilant.btnCopiazaClick(Sender: TObject);
var
  SrcAnexa, DestAnexa : Integer;
begin
  DestAnexa := qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
   with TfrmAnexaCopy.Create(Application) do
  try
    ShowModal;
    if ModalResult = mrOk then begin
      SrcAnexa := IdAnexeBilant;
      DBExecSQLFmt('exec [spTransferAnexa] %d, %d, %d, %d, %d ', [SrcAnexa, DestAnexa, Integer(BifaRanduri), Integer(BifaColoane), Integer(BifaFormule)]);
      IdAnexe :=  FIdAnexe;
    end;
  finally
    Free;
  end;
end;

end.
