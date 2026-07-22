unit PatchExcel;

interface
  function  IsExcelInstalled : Boolean;
  function WebPatchExcel : Boolean;

implementation

uses
  CommonDBVar, Registry, Windows, Dialogs, ComObj, ActiveX;

function WebPatchExcel : Boolean;
var
   lReg : TRegistry;
begin
   //modificam registri pentru a permite deschiderea fisierelor xls cu TWEBBrowser
(*
[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Word.Document.8]
"BrowserFlags"=dword:80000024

[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Word.Document.12]
"BrowserFlags"=dword:80000024


[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Excel.Sheet.8]
"BrowserFlags"=dword:80000A00

[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Excel.Sheet.12]
"BrowserFlags"=dword:80000A00
*)
(*
http://support.microsoft.com/kb/927009
*)

  lReg := TRegistry.Create(KEY_READ);
  try
    lReg.RootKey := HKEY_LOCAL_MACHINE;
    Result := False;
    if lReg.OpenKey('SOFTWARE\Classes\Excel.Sheet.8', False) then begin
      Result := (Cardinal(lReg.ReadInteger('BrowserFlags')) = $80000A00);
      lReg.CloseKey;
    end;
    if not Result then begin
      if not IsPowerUserLoggedOn and not IsAdminLoggedOn then
        ShowMessage('Excel-ul nu este configurat pentru vizualizare! '#13#10 +
                    'Va rugam contactati administratorul !' +#13#10+
                    'Administratorul poate modifica setarile prin rularea utilitarului de la adresa : http://support.microsoft.com/kb/927009'
                   )
      else begin
        lReg.Access := KEY_WRITE;
        try
          if lReg.OpenKey('SOFTWARE\Classes\Excel.Sheet.8', True) then begin
            lReg.WriteInteger('BrowserFlags', $80000A00);
            lReg.CloseKey;
          end;
          if lReg.OpenKey('SOFTWARE\Classes\Excel.Sheet.12', True) then begin
            lReg.WriteInteger('BrowserFlags', $80000A00);
            lReg.CloseKey;
          end;
          if lReg.OpenKey('SOFTWARE\Classes\Excel.SheetMacroEnabled.12', True) then begin
            lReg.WriteInteger('BrowserFlags', $80000A00);
            lReg.CloseKey;
          end;
          if lReg.OpenKey('SOFTWARE\Classes\Excel.SheetBinaryMacroEnabled.12', True) then begin
            lReg.WriteInteger('BrowserFlags', $80000A00);
            lReg.CloseKey;
          end;
          Result := True;
        except
          Result := False;
        end;
      end;
    end;
  finally
    lReg.Free;
  end;

end;

function  IsExcelInstalled : Boolean;
var
  ClassID: TCLSID;
  strOLEObject: string;
begin
  strOLEObject := 'Excel.Application';
  if (CLSIDFromProgID(PWideChar(WideString(strOLEObject)), ClassID) = S_OK) then  begin
    Result := True;
  end else begin
    Result := False;
  end;
end;


end.
