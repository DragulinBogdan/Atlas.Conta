unit ConcurentUsersUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxCntner, StdCtrls,  ExtCtrls, Db, ZDataSet, dxExEdtr, DxInspRw,
  DxDBInRw, DxInspct, DxDBInsp, dxDBTLCl, dxGrClms, dxTL, dxDBCtrl, dxDBGrid,
  Menus, cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels;

type
  TFrmConcurentUsers = class(TForm)
    pnClient: TPanel;
    ListaConexiuni: TdxDBGrid;
    InspDetalii: TdxDBInspector;
    Splitter1: TSplitter;
    DTUsers: TDataSource;
    InspDetaliiNUME_LOGIN: TdxInspectorDBMaskRow;
    InspDetaliiNUME_COMPLET: TdxInspectorDBMaskRow;
    InspDetaliiSTATIE: TdxInspectorDBMaskRow;
    InspDetaliiVERSION_ID: TdxInspectorDBMaskRow;
    InspDetaliiSPID: TdxInspectorDBMaskRow;
    InspDetaliiLOGIN_TIME: TdxInspectorDBDateRow;
    InspDetaliiHOSTNAME: TdxInspectorDBMaskRow;
    InspDetaliiPROGRAM_NAME: TdxInspectorDBMaskRow;
    InspDetaliiNT_DOMAIN: TdxInspectorDBMaskRow;
    InspDetaliiNT_USERNAME: TdxInspectorDBMaskRow;
    InspDetaliiNET_ADDRESS: TdxInspectorDBMaskRow;
    InspDetaliiIP_ADDRESS: TdxInspectorDBMaskRow;
    InspDetaliiIP_ADDRES: TdxInspectorDBMaskRow;
    ListaConexiuniNUME_LOGIN: TdxDBGridMaskColumn;
    ListaConexiuniNUME_COMPLET: TdxDBGridMaskColumn;
    ListaConexiuniSTATIE: TdxDBGridMaskColumn;
    ListaConexiuniVERSION_ID: TdxDBGridMaskColumn;
    ListaConexiuniSPID: TdxDBGridMaskColumn;
    ListaConexiuniLOGIN_TIME: TdxDBGridDateColumn;
    ListaConexiuniHOSTNAME: TdxDBGridMaskColumn;
    ListaConexiuniPROGRAM_NAME: TdxDBGridMaskColumn;
    ListaConexiuniNT_DOMAIN: TdxDBGridMaskColumn;
    ListaConexiuniNT_USERNAME: TdxDBGridMaskColumn;
    ListaConexiuniNET_ADDRESS: TdxDBGridMaskColumn;
    ListaConexiuniIP_ADDRESS: TdxDBGridMaskColumn;
    ListaConexiuniIP_ADDRES: TdxDBGridMaskColumn;
    BtnOk: TcxButton;
    BtnKill: TcxButton;
    procedure BtnOkClick(Sender: TObject);
    procedure BtnKillClick(Sender: TObject);
  private
    { Private declarations }
    IsSingleUser: Boolean;
  public
    { Public declarations }
  end;

function EnterSingleUser(AClass: TClass): Boolean;
procedure ExitSingleUser;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, CommonDBVar;

procedure ExitSingleUser;
begin
  DBExecSQL('exec spExitSingleUserMode');
end;

function EnterSingleUser(AClass: TClass): Boolean;

  function GetClassParam: String;
  begin
    if AClass <> nil then
      Result := AClass.ClassName
    else
      Result := '';
  end;

var
  lDataSet: TDataSet;
  lForm   : TFrmConcurentUsers;
begin
  lDataSet := DBNewQueryFmt('exec spCheckUserConcurency %d, %s', [IdUtilizator, GetClassParam]);
  try
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
      lForm := TFrmConcurentUsers.Create(nil);
      try
        lForm.DTUsers.DataSet := lDataSet;
        Result := lForm.ShowModal = mrOk;
      finally
        lForm.Free;
      end;
    end
    else
      Result := True;
  finally
    lDataSet.Free;
  end;
end;

procedure TFrmConcurentUsers.BtnOkClick(Sender: TObject);
begin
  if (IsSingleUser) and (not DTUsers.DataSet.IsEmpty) then
     if MessageDlg('Mai Exista utilizatori Conectati Cu Login-ul dumneavoastra'#13#10+
                   'Operatia care urmeaza nu permite acest lucru'#13#10+
                   'Doriti Abandonul Operatiei ?', mtConfirmation,
                   [mbYes, mbNo], 0) = mrYes then ModalResult := mrCancel
     else Abort
  else ModalResult := mrOk;
end;

procedure TFrmConcurentUsers.BtnKillClick(Sender: TObject);
var ANode: TdxTreeListNode;
begin
  aNode := ListaConexiuni.FocusedNode;
  if aNode <> nil then begin
    if MessageDlg('Sigur doriti sa deconectati conexiunea de la statia : '+aNode.Strings[ListaConexiuniSTATIE.Index]+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
      DBExecSQLFmt('kill %s', [aNode.Strings[ListaConexiuniSPID.Index]]);
      DTUsers.DataSet.Refresh;
    end;
  end;
end;

end.
