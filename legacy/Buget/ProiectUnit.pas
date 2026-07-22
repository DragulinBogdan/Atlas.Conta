unit ProiectUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, dxExEdtr, ImgList, dxCntner, dxDBTLCl,
  dxTL, dxDBCtrl, dxInspRw, dxDBInRw, dxDBInsp, dxInspct,
  ExtCtrls, dxEdLib, dxDBELib, dxEditor, Buttons, StdCtrls, ComCtrls, 
  DegradePanel, 
  cxControls, cxSplitter, dxDBTL,
  cxGraphics, cxLookAndFeelPainters,
  cxLookAndFeels;

type
  TfrmBugetProiect = class(TForm)
    StatusBar: TStatusBar;
    pnTopInfo: TPanel;
    pnContent: TPanel;
    StyleController: TdxEditStyleController;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    edtDirectie: TdxPopupEdit;
    edtProiect: TdxDBEdit;
    edtOrdonator: TdxEdit;
    pnInspector: TPanel;
    TreeProiecte: TdxDBTreeList;
    Imagini: TImageList;
    InspProiecte: TdxDBInspector;
    InspProiecteID_FUNCTIUNI: TdxInspectorDBMaskRow;
    InspProiecteID_PARINTE: TdxInspectorDBMaskRow;
    InspProiecteID_UTILIZATORI: TdxInspectorDBMaskRow;
    InspProiecteDENUMIRE: TdxInspectorDBMaskRow;
    InspProiecteDATA_INTRARE: TdxInspectorDBDateRow;
    InspProiecteDATA_IESIRE: TdxInspectorDBDateRow;
    InspProiecteSHAPE_TYPE: TdxInspectorDBMaskRow;
    InspProiecteSHAPE_LEFT_TOP: TdxInspectorDBMaskRow;
    InspProiecteSHAPE_RIGHT_BOTT: TdxInspectorDBMaskRow;
    InspProiecteSHAPE_COLOR: TdxInspectorDBMaskRow;
    InspProiecteSHAPE_FONT_COL: TdxInspectorDBMaskRow;
    InspProiecteSHAPE_FONT_NAME: TdxInspectorDBMaskRow;
    InspProiecteRow20: TdxInspectorDBRow;
    InspProiecteRow21: TdxInspectorDBRow;
    InspProiecteSTARE: TdxInspectorDBCheckRow;
    InspProiecteRow23: TdxInspectorDBRow;
    InspProiectePOS_ID: TdxInspectorDBSpinRow;
    InspProiecteDESCRIERE: TdxInspectorDBMemoRow;
    BtnAddProj: TSpeedButton;
    btnAddSubProj: TSpeedButton;
    btnDelProj: TSpeedButton;
    TreeProiecteDENUMIRE: TdxDBTreeListMaskColumn;
    TreeProiecteDATA_INTRARE: TdxDBTreeListDateColumn;
    TreeProiecteDATA_IESIRE: TdxDBTreeListDateColumn;
    TreeProiecteSTARE: TdxDBTreeListMaskColumn;         
    pnLeft: TPanel;
    TreeDirectii: TdxDBTreeList;
    TreeDirectiiID_BUGET_DIRECTII: TdxDBTreeListMaskColumn;
    TreeDirectiiID_BUGET_TIP_ORDONATOR: TdxDBTreeListMaskColumn;
    TreeDirectiiID_PARINTE: TdxDBTreeListMaskColumn;
    TreeDirectiiDENUMIRE: TdxDBTreeListMaskColumn;
    TreeDirectiiSTARE: TdxDBTreeListCheckColumn;

    edtChkFilter: TdxCheckEdit;
    pnTop: TDegradePanel;
    Splitter1: TcxSplitter;
    Splitter: TcxSplitter;
    procedure BtnAddProjClick(Sender: TObject);
    procedure btnAddSubProjClick(Sender: TObject);
    procedure btnDelProjClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure TreeProiecteGetImageIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeProiecteGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure edtDirectieInitPopup(Sender: TObject);
    procedure edtDirectieCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreeDirectiiChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
  protected
    { Private declarations }
    procedure SetFilter(IdDirectie : Integer; const Force : Boolean = False);
  public
    { Public declarations }
    constructor Create(AOwner: TComponent); override;
  end;

var
  fmBugetProiect: TfrmBugetProiect;

implementation

uses DateUnit, BugetContainer, ZDataSet;

{$R *.dfm}

procedure TfrmBugetProiect.BtnAddProjClick(Sender: TObject);
begin
  FrmData.QryBugetProiecte.Append;
  FrmData.QryBugetProiecte.FieldByName('ID_PARINTE').Clear;
  FrmData.QryBugetProiecte.Post;
end;

procedure TfrmBugetProiect.btnAddSubProjClick(Sender: TObject);
var
  ParentId : Variant;
begin
  if TreeProiecte.FocusedNode <> nil then ParentId := TdxDBTreeListNode(TreeProiecte.FocusedNode).Id
  else ParentId := Null;
  FrmData.QryBugetProiecte.Append;
  FrmData.QryBugetProiecte.FieldByName('ID_PARINTE').AsInteger := ParentId;
  FrmData.QryBugetProiecte.Post;
end;

procedure TfrmBugetProiect.btnDelProjClick(Sender: TObject);
var
  lProjName : String;
begin
 if TreeProiecte.FocusedNode = nil then
   MessageDlg('Cursorul trebuie plasat pe inregistrarea care se doreste stearsa !', mtError, [mbOK], 0)
 else begin
   lProjName := TreeProiecte.FocusedNode.Strings[TreeProiecteDENUMIRE.Index];
   if MessageDlg(Format('Doriti stegerea proiectului %s ?', [lProjName]), mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
   TdxDBTreeListNode(TreeProiecte.FocusedNode).Delete;
 end;
end;

constructor TfrmBugetProiect.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  edtDirectie.PopupControl := fmBugetContainer.TreeSelectDirectie;
end;

procedure TfrmBugetProiect.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmBugetProiect.TreeProiecteGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.Level < 2 then Index := Node.Level else Index := 1;
end;

procedure TfrmBugetProiect.TreeProiecteGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmBugetProiect.edtDirectieInitPopup(Sender: TObject);
begin
  TestAndCreatefmBugetContainer;
end;

procedure TfrmBugetProiect.edtDirectieCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);

var aNode : TdxDBTreeListNode;
begin
  if Accept then begin
   with TdxPopupEdit(Sender) do
       aNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(aNode) then begin
          Text := aNode.Strings[fmBugetContainer.TreeSelectDirectieDENUMIRE.Index];
          edtOrdonator.Text := '';
          if aNode.Strings[fmBugetContainer.TreeSelectDirectieTIP_ORDONATOR.Index] <> '' then
            edtOrdonator.Text := fmBugetContainer.TreeSelectDirectieTIP_ORDONATOR.Descriptions[
              aNode.Values[fmBugetContainer.TreeSelectDirectieTIP_ORDONATOR.Index] -1 
              ];
       end;
  end;
end;

procedure TfrmBugetProiect.TreeDirectiiChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  //CheckForSave

end;

procedure TfrmBugetProiect.SetFilter(IdDirectie: Integer;
  const Force: Boolean);
begin
  with FrmData.QryBugetProiecte do
  try
    DisableControls;
    Filtered := False;
    Filter := 'ID_BUGET_DIRECTII = ' + IntToStr(IdDirectie);
    Filtered := (edtChkFilter.Checked or Force) ;
  finally
    EnableControls;
  end;

end;

end.
