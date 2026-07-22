unit InchidereLunaUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, dxExEdtr, ExtCtrls, dxTL, dxCntner, dxDBCtrl, dxDBGrid, DB,
  ZDataSet, StdCtrls;

type
  TForm1 = class(TForm)
    Panel1: TPanel;
    DataSource1: TDataSource;
    ADOQuery1: TZQuery;
    ADOConnection1: TZConnection;
    dxDBGrid1: TdxDBGrid;
    dxDBGrid1key_field: TdxDBGridMaskColumn;
    dxDBGrid1Cont_Credit: TdxDBGridMaskColumn;
    dxDBGrid1Denumire_Credit: TdxDBGridMaskColumn;
    dxDBGrid1DENUMIRE_DEBIT: TdxDBGridMaskColumn;
    dxDBGrid1Cont_Debit: TdxDBGridMaskColumn;
    dxDBGrid1grup: TdxDBGridMaskColumn;
    dxDBGrid1Formula: TdxDBGridMaskColumn;
    dxDBGrid1Grupa: TdxDBGridMaskColumn;
    Panel2: TPanel;
    Memo1: TMemo;
    Label1: TLabel;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

{$R *.dfm}

end.
