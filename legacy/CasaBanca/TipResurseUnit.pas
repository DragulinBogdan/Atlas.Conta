unit TipResurseUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, HeadPanel, StdCtrls, Buttons, dxCntner, dxTL, dxDBCtrl,
  dxDBGrid, dxInspct, dxDBInsp, Db, ZDataSet, ToolWin, ComCtrls, ImgList,
  ActnList, dxInspRw, dxDBInRw;

type
  TFrmTipResurse = class(TForm)
    HeadPanel1: THeadPanel;
    pnContent: TPanel;
    pnBottom: TPanel;
    btnOk: TBitBtn;
    pnLeft: TPanel;
    Splitter1: TSplitter;
    pnRest: TPanel;
    pnTipResurse: TPanel;
    GridTipResurse: TdxDBGrid;
    pnCaracteristiciResurse: TPanel;
    GridCaracteristici: TdxDBGrid;
    pnCaractConfig: TPanel;
    DBInspector: TdxDBInspector;
    Splitter2: TSplitter;
    DTTipResursa: TDataSource;
    QryTipResursa: TZQuery;
    ToolBar: TToolBar;
    ImgList: TImageList;
    ActiuniTipResurse: TActionList;
    cmd_AddResource: TAction;
    cmd_DelResource: TAction;
    cmd_AddCarct: TAction;
    cmd_DelCaract: TAction;
    ToolButton1: TToolButton;
    ToolButton2: TToolButton;
    ToolButton3: TToolButton;
    ToolButton4: TToolButton;
    ToolButton5: TToolButton;
    GridTipResurseID_TIP_RESURSA: TdxDBGridMaskColumn;
    GridTipResurseDENUMIRE: TdxDBGridMaskColumn;
    QryCaracteristici: TZQuery;
    DTCaracteristici: TDataSource;
    GridCaracteristiciID_CARACTERISTICI_RESURSA: TdxDBGridMaskColumn;
    GridCaracteristiciID_TIP_RESURSA: TdxDBGridMaskColumn;
    GridCaracteristiciTIP_CARACTERISTICA: TdxDBGridMaskColumn;
    GridCaracteristiciNUME_CAMP: TdxDBGridMaskColumn;
    GridCaracteristiciTIP_CAMP: TdxDBGridMaskColumn;
    GridCaracteristiciCAPTURA_CULEGERE: TdxDBGridMaskColumn;
    GridCaracteristiciLISTA_VALORI: TdxDBGridMaskColumn;
    DBInspectorTIP_CARACTERISTICA: TdxInspectorDBMaskRow;
    DBInspectorNUME_CAMP: TdxInspectorDBMaskRow;
    DBInspectorTIP_CAMP: TdxInspectorDBMaskRow;
    DBInspectorCAPTURA_CULEGERE: TdxInspectorDBMaskRow;
    DBInspectorLISTA_VALORI: TdxInspectorDBMaskRow;
    procedure cmd_AddResourceExecute(Sender: TObject);
    procedure GridTipResurseChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure QryTipResursaAfterInsert(DataSet: TDataSet);
  private
    { Private declarations }
  public
    { Public declarations }
  end;


implementation

uses DateUnit;

{$R *.DFM}

procedure TFrmTipResurse.cmd_AddResourceExecute(Sender: TObject);
begin
  with QryTipResursa do begin
    if State in [dsEdit,dsInsert] then Post;
    Append;
  end;
end;

procedure TFrmTipResurse.GridTipResurseChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
var IdTipResursa : Integer;
begin
  IdTipResursa := 0;
  if Node.Strings[GridTipResurseID_TIP_RESURSA.Index] > '' then
    IdTipResursa := Node.Values[GridTipResurseID_TIP_RESURSA.Index];
  with QryCaracteristici do begin
    Close;
    Params.ParamByName('ID_TIP_RESUSRSA').Value := IdTipResursa;
    Open;
  end;
end;

procedure TFrmTipResurse.QryTipResursaAfterInsert(DataSet: TDataSet);
begin
  with DataSet do begin
     FieldByName('ID_TIP_RESURSA').AsInteger := GetNextId('TIP_RESURSA');
     FieldByName('DENUMIRE').AsString := 'TipResursa Noua';
  end;
end;

end.
