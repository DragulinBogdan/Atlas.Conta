unit ContUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, StdCtrls, dxExEdtr, dxEdLib, dxEditor, dxCntner, dxDBELib,
  dxfCheckBox, Menus, cxLookAndFeelPainters, cxButtons, cxGraphics, cxLookAndFeels,
  cxControls, cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, cxDBEdit, cxCheckBox, cxCurrencyEdit, cxMemo, cxCalendar;

type
  TfrmContProp = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    Label10: TLabel;
    Label11: TLabel;
    Label12: TLabel;
    Label13: TLabel;
    edSimbol: TcxDBMaskEdit;
    edDataSold: TcxDBDateEdit;
    edRomana: TcxDBMemo;
    edSID: TcxDBCurrencyEdit;
    edSIC: TcxDBCurrencyEdit;
    edTipCont: TcxDBImageComboBox;
    edSumator: TcxDBImageComboBox;
    edBalanta: TcxDBImageComboBox;
    edTip: TcxDBImageComboBox;
    ChkAplicaRecursiv: TcxCheckBox;
    Label4: TLabel;
    edTotalRepartitori: TcxCurrencyEdit;
    pnTop: TPanel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    procedure BtnCancelClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    FIntretin: TForm;
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses ZeosDBUtile, DB, dxCompsUtile, DateUnit, PlanConturiUnit, DateUtils;

procedure TfrmContProp.BtnCancelClick(Sender: TObject);
begin
  if FrmData.QryPlanCont.State in dsEditModes then
    FrmData.QryPlanCont.Cancel;
  ModalResult := mrCancel;
end;

procedure TfrmContProp.BtnOkClick(Sender: TObject);
var
  NewCont : String;
begin
  if FrmData.QryPlanCont.FieldByName('CONT').NewValue <> FrmData.QryPlanCont.FieldByName('CONT').OldValue then begin
    NewCont := FrmData.QryPlanCont.FieldByName('CONT').NewValue;

    if DBRecordExists('SELECT TOP 1 1 FROM CPLAN WHERE CONT = ''' + Trim(NewCont) + '''' ) then begin
      FrmData.QryPlanCont.Cancel;
      MessageDlg('Contul ' + NewCont + ' exista deja ! Operatia a fost anulata !!', mtError, [mbOK], 0);
      Abort;
    end;
  end;

  FrmData.QryPlanCont.Post;

  DBExecSQLFmt('exec spPlanUpdateDetalii %s, %s', [ValueToStr(FrmData.QryPlanCont['CONT']), ValueToStr(FrmData.QryPlanCont['PARINTE'])]);

  if ChkAplicaRecursiv.Checked then
     TFrmPlanConturi(FIntretin).SalveazaRecursiv(FrmData.QryPlanCont.FieldByName('CONT').AsString);
  ModalResult := mrOk;
end;

procedure TfrmContProp.FormCreate(Sender: TObject);
begin
  edDataSold.Date := EncodeDate(YearOf(Date), 01, 01);
  if DBProcExists('spCplanDefalcare') then
    FillImageCombo(edBalanta.Properties, 'exec [spCplanDefalcare]', 'tip', 'denumire');

end;

end.
 
