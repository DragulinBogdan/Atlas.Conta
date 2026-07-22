unit UnitParametrii;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Buttons, StdCtrls, ExtCtrls, cxControls, cxContainer, cxEdit, cxTextEdit,
  cxMaskEdit, cxButtonEdit, Menus, cxLookAndFeelPainters, cxButtons, ZConnection,
  cxGraphics, cxLookAndFeels;

type
  TFrmParametrii = class(TForm)
    LbInfo: TLabel;
    LbParam1: TLabel;
    BtnOk: TcxButton;
    RxSpeedButton1: TcxButton;
    LbParam2: TLabel;
    LbParam3: TLabel;
    LbParam4: TLabel;
    Panel1: TPanel;
    Label1: TLabel;
    EditParam1: TcxButtonEdit;
    EditParam2: TcxButtonEdit;
    EditParam3: TcxButtonEdit;
    EditParam4: TcxButtonEdit;
    procedure EditParam1Change(Sender: TObject);
    procedure EditParam1PropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
  private
    { Private declarations }
  public
    ADataBase: TZConnection;
    ATableName: String;
    Functie: String;    
    { Public declarations }
  end;

implementation

{$R *.DFM}



procedure TFrmParametrii.EditParam1Change(Sender: TObject);
var i:Integer;
begin
     i:=0;
     BtnOk.Visible:=True;
     while (BtnOk.Visible) and (i<ComponentCount) do
           begin
                if Components[i] is TcxButtonEdit then
                   BtnOk.Visible:=(not TcxButtonEdit(Components[i]).Visible) or (Trim(TcxButtonEdit(Components[i]).Text)<>'');
                i:=i+1;
           end;
end;

procedure TFrmParametrii.EditParam1PropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var S:String;
begin
   S:=TcxButtonEdit(Sender).Text;
//     if GetFormula(ADataBase,ATableName,'Functia : '+Functie,S,nil,TTipFormula(1-TcxButtonEdit(Sender).Tag)) then
//        TcxButtonEdit(Sender).Text:=S;
end;

end.
