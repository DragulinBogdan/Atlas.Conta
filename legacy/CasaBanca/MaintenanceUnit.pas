unit MaintenanceUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, CommonCasa,
  ComCtrls, StdCtrls, Buttons, ExtCtrls, dxCntner, dxEditor, dxExEdtr,
  dxEdLib;

type
  TFrmSettings = class(TForm)
    SettingsControl: TPageControl;
    pnBottom: TPanel;
    btnOk: TBitBtn;
    btnCancel: TBitBtn;
    tabSettings: TTabSheet;
    pn_FirstLevelColor: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    pn_SecondLevelColor: TPanel;
    Label3: TLabel;
    pn_DeletedSecondLevelColor: TPanel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    pn_ParentColor: TPanel;
    Label8: TLabel;
    Bevel2: TBevel;
    ColorDialog: TColorDialog;
    FontDialog: TFontDialog;
    tabTransfer: TTabSheet;
    pn_ChildColor: TPanel;
    Label10: TLabel;
    Bevel4: TBevel;
    pn_DataColor: TPanel;
    Label14: TLabel;
    rbColor: TRadioButton;
    rbFont: TRadioButton;
    Label20: TLabel;
    pn_FocusedColor: TPanel;
    tabSetariGrid: TTabSheet;
    chkAutomat: TCheckBox;
    Label21: TLabel;
    edtSearchType: TdxImageEdit;
    Label7: TLabel;
    Bevel1: TBevel;
    Label9: TLabel;
    Bevel3: TBevel;
    edtNrDecimal: TdxSpinEdit;
    Label25: TLabel;
    Label26: TLabel;
    pnDLevel4B: TPanel;
    Label27: TLabel;
    pnDLevel3B: TPanel;
    Label28: TLabel;
    pnDLevel4A: TPanel;
    Label29: TLabel;
    pnDLevel3A: TPanel;
    pnDLevel2: TPanel;
    Label30: TLabel;
    Label31: TLabel;
    pnDLevel1: TPanel;
    Label32: TLabel;
    Label33: TLabel;
    Label34: TLabel;
    pn_Validare: TPanel;
    btnDefault: TBitBtn;
    Label35: TLabel;
    Label36: TLabel;
    edtModVizTransfer: TdxImageEdit;
    Label37: TLabel;
    Label38: TLabel;
    edtDispRaport: TdxButtonEdit;
    chkDisplayStr: TCheckBox;
    tabSaveSheet: TTabSheet;
    Label11: TLabel;
    Bevel5: TBevel;
    chkSaveCasaDefault: TCheckBox;
    chkSavePeZi: TCheckBox;
    chkSaveZileAnt: TCheckBox;
    chkSaveTransfImg: TCheckBox;
    chkSaveDataStart: TCheckBox;
    chkSaveTipDefalcare: TCheckBox;
    procedure FormCreate(Sender: TObject);
    procedure chkAutomatClick(Sender: TObject);
    procedure edtSearchTypeChange(Sender: TObject);
    procedure btnDefaultClick(Sender: TObject);
    procedure edtModVizTransferChange(Sender: TObject);
    procedure edtDispRaportButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure chkDisplayStrClick(Sender: TObject);
    procedure chkSaveCasaDefaultClick(Sender: TObject);
    procedure edtNrDecimalChange(Sender: TObject);
  private
    { Private declarations }
  protected
    procedure ChangeColor(Sender: TObject);
    procedure ChangeFont(Sender: TObject);
    procedure ChangeSomething(Sender: TObject; Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
    procedure ChangeImage(Sender : TObject);
    procedure CreateDisplaySettings(tab : TTabSheet);
  public
    { Public declarations }
    procedure ApplyCurrentSettings(Default : Boolean = False);
    procedure SaveSettings;
  end;

implementation

uses ChouseReportUnit, DateUnit;

{$R *.DFM}

{ TFrmSettings }
procedure TFrmSettings.ApplyCurrentSettings(Default : Boolean = False);
begin
  ReadSettingsRegistru(Default);
  CreateDisplaySettings(tabTransfer);
  if Assigned(ListOfCustomColors) then
    ColorDialog.CustomColors.Assign(ListOfCustomColors);

  pn_FirstLevelColor.Color    := cl_FirstLevelColor;
  pn_SecondLevelColor.Color   := cl_SecondLevelColor;
  pn_DeletedSecondLevelColor.Color := cl_DeletedSecondLevelColor;

  pn_ChildColor.Color := cl_ChildColor;
  pn_ParentColor.Color := cl_ParentColor;
  pn_DataColor.Color := cl_DataColor;

  pn_FocusedColor.Color := cl_FocusedColor;

  SetConstantToFont(pn_FirstLevelColor.Font, ft_FirstLevelColor);
  SetConstantToFont(pn_SecondLevelColor.Font, ft_SecondLevelColor);
  SetConstantToFont(pn_DeletedSecondLevelColor.Font, ft_DeletedSecondLevelColor);

  SetConstantToFont(pn_ChildColor.Font, ft_ChildColor);
  SetConstantToFont(pn_ParentColor.Font, ft_ParentColor);

  SetConstantToFont(pn_DataColor.Font, ft_DataColor);
  SetConstantToFont(pn_FocusedColor.Font, ft_FocusedColor);

{partea de deconturi}
  {culori}
  pnDLevel1.Color := clDLevel1;
  pnDLevel2.Color := clDLevel2;
  pnDLevel3A.Color := clDLevel3A;
  pnDLevel3B.Color := clDLevel3B;
  pnDLevel4A.Color := clDLevel4A;
  pnDLevel4B.Color := clDLevel4B;
  {font}
  SetConstantToFont(pnDLevel1.Font, ftDLevel1);
  SetConstantToFont(pnDLevel2.Font, ftDLevel2);
  SetConstantToFont(pnDLevel3A.Font, ftDLevel3A);
  SetConstantToFont(pnDLevel3B.Font, ftDLevel3B);
  SetConstantToFont(pnDLevel4A.Font, ftDLevel4A);
  SetConstantToFont(pnDLevel4B.Font, ftDLevel4B);
{end partea de deconturi}

  chkAutomat.Checked := IsOnQuestion;

  edtSearchType.Text := IntToStr(GetIntFromSearchType(ModDeCautare));
  edtModVizTransfer.Text := IntToStr(GetIntFromDispType(ModAfisTranfer));
  chkDisplayStr.Checked := ModAfisTree;
  
  edtNrDecimal.IntValue := CurrDecimal;

  pn_Validare.Color := cl_Validare;
  SetConstantToFont(pn_Validare.Font, ft_Validare);


  {partea de setari}
    chkSaveCasaDefault.OnClick := nil;
    chkSaveCasaDefault.Checked := IsSaveCasaDefault;
    chkSavePeZi.Checked := IsSavePeZi;
    chkSaveZileAnt.Checked := IsSaveZileAnt;
    chkSaveTransfImg.Checked := IsSaveTransfImg;
    chkSaveDataStart.Checked := IsSaveDataStart;
    chkSaveTipDefalcare.Checked := IsSaveTipDefalcare;
    chkSaveCasaDefault.OnClick := chkSaveCasaDefaultClick;
  {endparte de setari}


  with TFrmChouseReport.Create(Self) do
    try
      QryReports.Connection := FrmData.dbContabilitate;
      QryReports.Open;
      SetReportLists(IntToStr(rb_DispozitieId));
      if (Trim(GetReportLists)<>'') and (rb_DispozitieId = CurentReportId) then begin
          edtDispRaport.Text := CurentReport;
       end
      else begin
        edtDispRaport.Text := '';
        rb_DispozitieId := -1;
      end;
    finally
      Free;
    end;




end;

procedure TFrmSettings.ChangeColor(Sender: TObject);
var aColor : TColor;
begin
  if not Assigned(Sender) then Exit;
  ColorDialog.Color := TPanel(Sender).Color;
  if Assigned(ListOfCustomColors) then
    ColorDialog.CustomColors.Assign(ListOfCustomColors);
  if ColorDialog.Execute then begin
    aColor := ColorDialog.Color;
    with (Sender as TPanel) do begin
      Color := aColor;
    if Assigned(ListOfCustomColors) then
      ListOfCustomColors.Assign(ColorDialog.CustomColors);
    end;
  end;
end;

procedure TFrmSettings.ChangeFont(Sender: TObject);
var aFont : TFont;
begin
  if not Assigned(Sender) then Exit;
  FontDialog.Font := TPanel(Sender).Font;
  if FontDialog.Execute then begin
    aFont := FontDialog.Font;
    with (Sender as TPanel) do begin
      Font := aFont;
    end;
  end;
end;

procedure TFrmSettings.ChangeSomething(Sender: TObject; Button: TMouseButton;
  Shift: TShiftState; X, Y: Integer);
begin
  if ((rbColor.Checked) and (Button = mbLeft)) or ((Button = mbRight) and (rbFont.Checked)) then ChangeColor(Sender)
  else ChangeFont(Sender);
  NeedToWrite := True;
end;

procedure TFrmSettings.FormCreate(Sender: TObject);
var I : Integer;
begin
  SettingsControl.ActivePage := tabSettings;
  for I := 0 to ComponentCount - 1 do
    if Components[I] is TPanel then
      if TPanel(Components[I]).Tag > 0 then TPanel(Components[I]).OnMouseup := ChangeSomething;
end;

procedure TFrmSettings.SaveSettings;
begin
  cl_FirstLevelColor := pn_FirstLevelColor.Color;
  cl_SecondLevelColor := pn_SecondLevelColor.Color;
  cl_DeletedSecondLevelColor := pn_DeletedSecondLevelColor.Color;

  cl_ChildColor := pn_ChildColor.Color;
  cl_ParentColor := pn_ParentColor.Color;
  cl_DataColor   := pn_DataColor.Color;

  cl_FocusedColor := pn_FocusedColor.Color;

  SetFontToConstant(pn_FirstLevelColor.Font, ft_FirstLevelColor);
  SetFontToConstant(pn_SecondLevelColor.Font, ft_SecondLevelColor);
  SetFontToConstant(pn_DeletedSecondLevelColor.Font, ft_DeletedSecondLevelColor);

  SetFontToConstant(pn_ChildColor.Font, ft_ChildColor);
  SetFontToConstant(pn_ParentColor.Font, ft_ParentColor);

  SetFontToConstant(pn_DataColor.Font, ft_DataColor);
  SetFontToConstant(pn_FocusedColor.Font, ft_FocusedColor);


{partea de decontari)
  {culori}
  clDLevel1  := pnDLevel1.Color;
  clDLevel2  := pnDLevel2.Color;
  clDLevel3A := pnDLevel3A.Color;
  clDLevel3B := pnDLevel3B.Color;
  clDLevel4A := pnDLevel4A.Color;
  clDLevel4B := pnDLevel4B.Color;
  {font}
  SetFontToConstant(pnDLevel1.Font, ftDLevel1);
  SetFontToConstant(pnDLevel2.Font, ftDLevel2);
  SetFontToConstant(pnDLevel3A.Font, ftDLevel3A);
  SetFontToConstant(pnDLevel3B.Font, ftDLevel3B);
  SetFontToConstant(pnDLevel4A.Font, ftDLevel4A);
  SetFontToConstant(pnDLevel4B.Font, ftDLevel4B);
{end partea de deconturi}

  IsOnQuestion := chkAutomat.Checked;
  ModDeCautare := GetSearchTypeFromInt(StrToInt(edtSearchType.Text));
  ModAfisTranfer := GetDispTypeFromInt(StrToInt(edtModVizTransfer.Text));

  CurrDecimal := edtNrDecimal.IntValue;

  cl_Validare := pn_Validare.Color;
  SetFontToConstant(pn_Validare.Font, ft_Validare);

  if Assigned(ListOfCustomColors) then
    ListOfCustomColors.Assign(ColorDialog.CustomColors);

  WriteSettingsRegistru;
end;

procedure TFrmSettings.chkAutomatClick(Sender: TObject);
begin
  IsOnQuestion := chkAutomat.Checked;
  NeedToWrite := True;
end;

procedure TFrmSettings.edtSearchTypeChange(Sender: TObject);
begin
  ModDeCautare := GetSearchTypeFromInt(StrToInt(edtSearchType.Text));
  NeedToWrite := True;
end;

procedure TFrmSettings.btnDefaultClick(Sender: TObject);
begin
  ApplyCurrentSettings(True);
end;

procedure TFrmSettings.edtModVizTransferChange(Sender: TObject);
begin
   ModAfisTranfer := GetDispTypeFromInt(StrToInt(edtModVizTransfer.Text));
   NeedToWrite := True;
end;

procedure TFrmSettings.edtDispRaportButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var aRep : String;
begin
  with TFrmChouseReport.Create(Self) do
    try
      MultiSelect := False;
      QryReports.Connection := FrmData.dbContabilitate;
      QryReports.Open;
      aRep := IntToStr(rb_DispozitieId);
      SetReportLists(aRep);
      rb_DispozitieId := -1;
      if ShowModal = mrOk then begin
        aRep := GetReportLists;
        if Trim(aRep)<>'' then begin
          edtDispRaport.Text := CurentReport;
          rb_DispozitieId := CurentReportId;
        end;
      end;
    finally
      Free;
    end;
  NeedToWrite := True;
end;

procedure TFrmSettings.chkDisplayStrClick(Sender: TObject);
begin
  ModAfisTree := chkDisplayStr.Checked;
  NeedToWrite := True;
end;

procedure TFrmSettings.CreateDisplaySettings(tab: TTabSheet);
var
  I : Integer;
  aPanel : TPanel;
  aImage : TdxGraphicEdit;
begin
  for I := Low(TransferDiplayTable) to High(TransferDiplayTable) do begin
    aPanel := TPanel(tab.FindComponent(Format('pn_Transfer%d', [I])));
    if aPanel = nil then begin
       aPanel := TPanel.Create(tab);
       with aPanel do begin
          Parent := Tab;
          Name := Format('pn_Transfer%d', [I]);
          Tag := 1;
          Width := 299;
          Top := 13 + (I* 23);
          Height := 20;
          Left := 94;
          Alignment := taLeftJustify;
          BevelInner := bvLowered;
          BevelOuter := bvNone;
          Caption := TransferDiplayTable[I].Nume;
          OnMouseUp := ChangeSomething;
       end;

       with TLabel.Create(tab) do begin
         Parent := tab;
         Name := Format('lbl_Transfer%d', [I]);
         Left := 30;
         Top := 13 + (I* 23);
         Width := 48;
         Height := 20;
         Caption := Format('Transfer %d', [I]);
       end;
    end;

    aImage := TdxGraphicEdit(tab.FindComponent(Format('img_Transfer%d', [I])));
    if aImage = nil then begin
      aImage := TdxGraphicEdit.Create(tab);
      with aImage do begin
        Parent := tab;
        Name := Format('img_Transfer%d', [I]);
        Left := 6;
        Top := 13 + (I * 23);
        Width := 20;
        Height := 20;
        Center := True;
        Stretch := True;
        Caption := '';
        Tag := I;
        GraphicTransparency := gtTransparent;
      end;
    end;
    aImage.OnChange := nil;
    FrmData.ImaginiTransfer.GetBitmap(I, aImage.Picture.Bitmap);
    aImage.OnChange := ChangeImage;

    with aPanel do begin
      Caption := TransferDiplayTable[I].Nume;
      SetConstantToFont(Font,TransferDiplayTable[I].FontInfo);
      Color := TransferDiplayTable[I].Color;
    end;
  end;
end;

procedure TFrmSettings.ChangeImage(Sender: TObject);
var
   lImage : TdxGraphicEdit;
   aBitmap : TBitmap;
begin
  if not Assigned(Sender) then Exit;
  if not (Sender is TdxGraphicEdit) then Exit;
  lImage := TdxGraphicEdit(Sender);

  try
    lImage.OnChange := nil;
    aBitmap := TBitmap.Create;
    FrmData.ImaginiTransfer.GetBitmap(lImage.Tag, aBitmap);
    aBitmap.Canvas.StretchDraw(aBitmap.Canvas.ClipRect, lImage.Picture.Graphic);
    if aBitmap.Empty then
      FrmData.ImaginiTransfer.ReplaceMasked(lImage.Tag, nil, clDefault)
    else
      FrmData.ImaginiTransfer.ReplaceMasked(lImage.Tag, aBitmap, aBitmap.TransparentColor);

    {debug}
    FrmData.ImaginiTransfer.GetBitmap(lImage.Tag, lImage.Picture.Bitmap);
  finally
    if aBitmap <> nil then FreeAndNil(aBitmap);
    lImage.OnChange := ChangeImage;
  end;
  if IsSaveTransfImg then
    NeedToWrite := True;
end;

procedure TFrmSettings.chkSaveCasaDefaultClick(Sender: TObject);
begin
  if not Assigned(chkSaveCasaDefault.OnClick) then Exit;
  //
  IsSaveCasaDefault := chkSaveCasaDefault.Checked;
  IsSavePeZi := chkSavePeZi.Checked;
  IsSaveZileAnt := chkSaveZileAnt.Checked;
  IsSaveTransfImg := chkSaveTransfImg.Checked;
  IsSaveDataStart := chkSaveDataStart.Checked;
  NeedToWrite := True;
  IsSaveTipDefalcare := chkSaveTipDefalcare.Checked;

end;

procedure TFrmSettings.edtNrDecimalChange(Sender: TObject);
begin
  NeedToWrite := True;
end;

end.
