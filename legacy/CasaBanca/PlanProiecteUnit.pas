unit PlanProiecteUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ComCtrls, ImgList, StdCtrls,
  ExtCtrls, dxInspRw, dxDBInRw, dxExEdtr,
  dxInspct, dxCntner, dxDBInsp, dxTL, dxDBCtrl,
  dxDBCtrl, dxDBTLCl, AtsInRwEx;

type
  TFrmPlanProiecte = class(TForm)
    ImaginiConturi: TImageList;
    GrPlanBugete: TGroupBox;
    TreeProiecte: TdxDBTreeList;
    Splitter1: TSplitter;
    TreeProiecteBUGET: TdxDBTreeListMaskColumn;
    TreeProiecteROMANA: TdxDBTreeListMaskColumn;
    TreeProiecteREALIZATPROC: TdxDBTreeListColumn;
    TreeProiectePLANIFICAT: TdxDBTreeListCurrencyColumn;
    pnRight: TPanel;
    ProiectParams: TdxDBInspector;
    TreeProiecterREALIZAT: TdxDBTreeListCurrencyColumn;
    ProiectParamsDENUMIRE: TdxInspectorDBMaskRow;
    ProiectParamsDATA_LIMITA: TdxInspectorDBDateRow;
    ProiectParamsSTARE: TdxInspectorDBMaskRow;
    ProiectParamsBUGET: TdxInspectorDBMaskRow;
    ProiectParamsREALIZAT: TdxInspectorDBMaskRow;
    ProiectParamsRow9: TdxInspectorDBRow;
    ProiectParamsRow10: TdxInspectorDBRow;
    ProiectParamsRow11: TdxInspectorDBRow;
    ProiectParamsRow12: TdxInspectorDBRow;
    ProiectParamsID_TIP_PROIECTE: TdxInspectorDBImageRow;
    pnTop: TPanel;
    procedure TreeProiecteGetImageIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeProiecteGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeProiecteREALIZATPROCCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure TreeProiecteBUGETGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeBugeteREALIZAT1CustomDrawColumnHeader(Sender: TObject;
      AColumn: TdxTreeListColumn; ACanvas: TCanvas; ARect: TRect;
      var AText: String; var AColor: TColor; AFont: TFont;
      var AAlignment: TAlignment; var ASorted: TdxTreeListColumnSort;
      var ADone: Boolean);
    procedure ProiectParamsEditing(Sender: TObject; Node: TdxInspectorNode;
      Row: TdxInspectorRow; var Allow: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    procedure AppIdle(Sender: TObject; var Done: Boolean);
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses DateUnit, CommonCasa;

procedure TFrmPlanProiecte.TreeProiecteGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.HasChildren then
     if Node.Expanded then
        Index := 2
     else
        Index := 0
  else
     Index := 1;
end;

procedure TFrmPlanProiecte.TreeProiecteGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TFrmPlanProiecte.TreeProiecteREALIZATPROCCustomDrawCell(
  Sender: TObject; ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var Procent: Integer;
begin
  { Setam un progress bar pentru pozitia curenta }
  Procent := 0;
  if (aNode<> nil) and (aColumn = TreeProiecteREALIZATPROC) then begin
     if ANode.Strings[TreeProiectePLANIFICAT.Index]>'' then
       if aNode.Values[TreeProiectePLANIFICAT.Index] > 0 then
          Procent := aNode.Values[TreeProiecterREALIZAT.Index]/aNode.Values[TreeProiectePLANIFICAT.Index]*10000
       else Procent := 0;
       DrawProcent(aCanvas, aRect, Procent, clWhite, clNavy);
       aDone := True;
  end;
end;

procedure TFrmPlanProiecte.TreeProiecteBUGETGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  aText := aText +'  :  ('+aNode.Strings[TreeProiecteROMANA.Index]+' )';
end;

procedure TFrmPlanProiecte.TreeBugeteREALIZAT1CustomDrawColumnHeader(
  Sender: TObject; AColumn: TdxTreeListColumn; ACanvas: TCanvas;
  ARect: TRect; var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ASorted: TdxTreeListColumnSort;
  var ADone: Boolean);
begin
  aFont.Style := aFont.Style + [fsBold];
  aFont.Color := clNavy;
end;


procedure TFrmPlanProiecte.ProiectParamsEditing(Sender: TObject;
  Node: TdxInspectorNode; Row: TdxInspectorRow; var Allow: Boolean);
begin
  Allow := (Row <> nil) and (Row.Tag = 0);
end;

procedure TFrmPlanProiecte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TFrmPlanProiecte.FormCreate(Sender: TObject);
begin
  PopulateImage(FrmData.QryTipProiect,
       ProiectParamsID_TIP_PROIECTE.Values,
       ProiectParamsID_TIP_PROIECTE.Descriptions,
       'ID_TIP_PROIECTE',
       'DENUMIRE');

  Application.OnIdle := AppIdle;
end;

procedure TFrmPlanProiecte.AppIdle(Sender: TObject; var Done: Boolean);
begin
   Application.ProcessMessages;
   if not Assigned(TreeProiecte.FocusedNode) then Exit;
   ProiectParamsID_TIP_PROIECTE.Visible := not(TreeProiecte.FocusedNode.HasChildren);
end;

procedure TFrmPlanProiecte.FormDestroy(Sender: TObject);
begin
  Application.OnIdle := nil;
end;

end.
