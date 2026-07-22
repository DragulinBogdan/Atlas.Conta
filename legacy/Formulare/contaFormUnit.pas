unit contaFormUnit;

interface

uses
  Forms;

type
  TTipFormular = (tfFormTab, tfFormModal, tfFormStick);

  TFormular = class(TForm)
  public
    class function TipFormular: TTipFormular;
  end;

implementation

uses
  Variants;

{ TFormular }

class function TFormular.TipFormular: TTipFormular;
begin

end;

end.
