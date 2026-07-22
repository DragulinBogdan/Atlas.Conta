unit BugetContainer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, dxExEdtr, dxCntner, dxTL, dxDBCtrl, 
  Menus, cxLookAndFeelPainters, dxEditor, dxEdLib, dxDBELib, dxInspRw,
  dxDBInRw, dxInspct, dxDBInsp, StdCtrls, cxButtons, ExtCtrls, dxDBTLCl,
  dxDBTL,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmBugetContainer = class(TForm)
    TreeSelectDirectie: TdxDBTreeList;
    TreeSelectDirectieID_BUGET_DIRECTII: TdxDBTreeListMaskColumn;
    TreeSelectDirectieID_PARINTE: TdxDBTreeListMaskColumn;
    TreeSelectDirectieDENUMIRE: TdxDBTreeListMaskColumn;
    TreeSelectDirectieDESCRIERE: TdxDBTreeListMaskColumn;
    TreeSelectDirectieATRIBUTII: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSTARE: TdxDBTreeListCheckColumn;
    TreeSelectDirectieDATA_START_FUNCTIONARE: TdxDBTreeListDateColumn;
    TreeSelectDirectieDATA_STOP_FUNCTIONARE: TdxDBTreeListDateColumn;
    TreeSelectDirectieSHAPE_TYPE: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSHAPE_LEFT_TOP: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSHAPE_RIGHT_BOTT: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSHAPE_COLOR: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSHAPE_FONT_COL: TdxDBTreeListMaskColumn;
    TreeSelectDirectieSHAPE_FONT_NAME: TdxDBTreeListMaskColumn;
    TreeSelectDirectiePOS_ID: TdxDBTreeListMaskColumn;
    TreeSelectDirectieID_UTILIZATORI: TdxDBTreeListMaskColumn;
    TreeSelectDirectieTIP_ORDONATOR: TdxDBTreeListImageColumn;
    pnOrdonantator: TPanel;
    Label1: TLabel;
    SpeedButton1: TcxButton;
    inspOrdonantatori: TdxDBInspector;
    inspOrdonantatoriNUME: TdxInspectorDBMaskRow;
    inspOrdonantatoriPRENUME: TdxInspectorDBMaskRow;
    inspOrdonantatoriDATA: TdxInspectorDBDateRow;
    inspOrdonantatoriTIP: TdxInspectorDBMaskRow;
    ieOrdonantatori: TdxDBImageEdit;
    procedure TreeSelectDirectieDblClick(Sender: TObject);
    procedure TreeSelectDirectieKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;


procedure TestAndCreatefmBugetContainer;


var
  fmBugetContainer: TfrmBugetContainer;

implementation

uses DateUnit;




{$R *.dfm}

procedure TestAndCreatefmBugetContainer;
begin
  if fmBugetContainer = nil then
    fmBugetContainer := TfrmBugetContainer.Create(nil);
end;


procedure TfrmBugetContainer.TreeSelectDirectieDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and ((TdxDBTreeList(Sender).Tag = 1) or not FocusedNode.HasChildren) then begin
      (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
    end;
end;

procedure TfrmBugetContainer.TreeSelectDirectieKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
  begin
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
  end;
end;

procedure TfrmBugetContainer.FormCreate(Sender: TObject);
begin
  PopulateImage(FrmData.QryBugetTipOrdonator,
              TreeSelectDirectieTIP_ORDONATOR.Values,
              TreeSelectDirectieTIP_ORDONATOR.Descriptions,
              'ID_BUGET_TIP_ORDONATOR', 'DENUMIRE');
end;

initialization
   fmBugetContainer := nil;
end.
