unit AntetUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, HeadPanel, cxLookAndFeelPainters, StdCtrls, cxButtons,
  DB, ZDataSet, Menus, cxGraphics,
  cxLookAndFeels, cxControls, cxPC, cxStyles, cxEdit, cxImage, cxVGrid,
  cxDBVGrid, cxInplaceContainer, cxTextEdit, dxScrollbarAnnotations;

type
  TfrmIntretinereAntet = class(TForm)
    HeadPanel1: THeadPanel;
    pnBottom: TPanel;
    AntetRecord: TcxDBVerticalGrid;
    AntetRecordCategoryRow1: TcxCategoryRow;
    AntetRecordCategoryRow2: TcxCategoryRow;
    AntetRecordCategoryRow3: TcxCategoryRow;
    AntetRecordSemnaturi: TcxCategoryRow;
    AntetRecordCategoryRow5: TcxCategoryRow;
    AntetRecordID_ANTET_DATE_SOCIETATE: TcxDBEditorRow;
    AntetRecordNUME_SOCIETATE: TcxDBEditorRow;
    AntetRecordPERS_CONTACT: TcxDBEditorRow;
    AntetRecordADRESA_SOCIETATE: TcxDBEditorRow;
    AntetRecordCOD_FISCAL: TcxDBEditorRow;
    AntetRecordTELEFON: TcxDBEditorRow;
    AntetRecordLOCALITATE: TcxDBEditorRow;
    AntetRecordJUDET: TcxDBEditorRow;
    AntetRecordFAX: TcxDBEditorRow;
    AntetRecordEMAIL: TcxDBEditorRow;
    AntetRecordSTRADA: TcxDBEditorRow;
    AntetRecordNR_POSTAL: TcxDBEditorRow;
    AntetRecordBLOC: TcxDBEditorRow;
    AntetRecordSCARA: TcxDBEditorRow;
    AntetRecordAP: TcxDBEditorRow;
    AntetRecordIMAGINE: TcxDBEditorRow;
    AntetRecordIMAGINE_CLASS: TcxDBEditorRow;
    AntetRecordADRESA_WEB: TcxDBEditorRow;
    AntetRecordINSTITUTIE_SUPERIOARA: TcxDBEditorRow;
    BtnOk: TcxButton;
    btnCreateSemnatura: TcxButton;
    procedure btnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure cxDBVerticalGrid1IMAGINEEditPropertiesAssignPicture(
      Sender: TObject; const Picture: TPicture);
    procedure cxDBVerticalGrid1IMAGINEEditPropertiesGetGraphicClass(
      AItem: TObject; ARecordIndex: Integer; APastingFromClipboard: Boolean;
      var AGraphicClass: TGraphicClass);
    procedure btnCreateSemnaturaClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
    procedure CreateSemnaturi;
    procedure RefreshDataSet;
  public
    { Public declarations }
  end;

procedure ShowIntretinereAntet;

implementation

uses
  ZeosDBUtile, DateUnit, ATSZDBUtils,  GraphicEx;

{$R *.dfm}

procedure ShowIntretinereAntet;
var
  lfmIntretinereAntet: TfrmIntretinereAntet;
begin
  lfmIntretinereAntet := TfrmIntretinereAntet.Create(nil);
  try
    lfmIntretinereAntet.ShowModal;
  finally
    lfmIntretinereAntet.Free;
  end;
end;

procedure TfrmIntretinereAntet.btnCreateSemnaturaClick(Sender: TObject);
begin
  if frmData.QryAntetUnitate.State in [dsEdit, dsInsert] then frmData.QryAntetUnitate.Post;
  DBExecSQL('exec spAntetCreateSemnatura');
  //golim cacheul
  frmData.dbContabilitate.DbcConnection.GetMetadata.ClearCache;
  RefreshDataSet;
end;

procedure TfrmIntretinereAntet.btnOkClick(Sender: TObject);
begin
  if frmData.QryAntetUnitate.State in [dsEdit, dsInsert] then frmData.QryAntetUnitate.Post;
  GetHeaderSocietate;
  ModalResult := mrOk;
end;

procedure TfrmIntretinereAntet.CreateSemnaturi;
var
   I : Integer;
   RecordCatSemn: TcxCategoryRow;
   RecordRow : TcxDBEditorRow;
begin
  with frmData.QryAntetUnitate do
  for I := 1 to 20 do begin
    if  (FindField('USER_FUNCTIE' + IntToStr(I)) <> nil) and
        (FindField('USER_NUME' + IntToStr(I)) <> nil) and
        (AntetRecord.RowByCaption('Semnaturi ' + IntToStr(I)) = nil)
    then begin

      RecordCatSemn := TcxCategoryRow(AntetRecord.AddChild(AntetRecordSemnaturi, TcxCategoryRow));
      RecordCatSemn.Properties.Caption := 'Semnaturi ' + IntToStr(I);

      RecordRow := TcxDBEditorRow(AntetRecord.AddChild(RecordCatSemn, TcxDBEditorRow));
      RecordRow.Properties.Caption := 'Functie';
      RecordRow.Properties.DataBinding.FieldName := 'USER_FUNCTIE' + IntToStr(I);

      RecordRow := TcxDBEditorRow(AntetRecord.AddChild(RecordCatSemn, TcxDBEditorRow));
      RecordRow.Properties.Caption := 'Nume';
      RecordRow.Properties.DataBinding.FieldName := 'USER_NUME' + IntToStr(I);
    end;
  end;

end;

procedure TfrmIntretinereAntet.cxDBVerticalGrid1IMAGINEEditPropertiesAssignPicture(
  Sender: TObject; const Picture: TPicture);
begin
  with frmData.QryAntetUnitate do begin
    if not (State in [dsEdit, dsInsert]) then
       Edit;
    if Picture.Graphic <> nil then
       FieldByName('IMAGINE_CLASS').AsString := Picture.Graphic.ClassName
    else
       FieldByName('IMAGINE_CLASS').Clear;
  end;
end;

procedure TfrmIntretinereAntet.cxDBVerticalGrid1IMAGINEEditPropertiesGetGraphicClass(
  AItem: TObject; ARecordIndex: Integer; APastingFromClipboard: Boolean;
  var AGraphicClass: TGraphicClass);
begin
  with frmData.QryAntetUnitate do
    AGraphicClass := FileFormatList.FindGraphicByName(FieldByName('IMAGINE_CLASS').AsString);
end;

procedure TfrmIntretinereAntet.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmIntretinereAntet.RefreshDataSet;
begin
  if frmData.QryAntetUnitate.Active then frmData.QryAntetUnitate.Active := False;
  frmData.QryAntetUnitate.Active := True;
  CreateSemnaturi;
  if frmData.QryAntetUnitate.IsEmpty then begin
    frmData.QryAntetUnitate.Append;
    frmData.QryAntetUnitate.FieldByName('IMAGINE_CLASS').AsString := '';
    frmData.QryAntetUnitate.Post;
  end;
end;

procedure TfrmIntretinereAntet.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

end.
