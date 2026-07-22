unit WebNavUtils;

interface

uses
  Classes, Controls, Forms, SHDocVw, Vcl.StdCtrls, Vcl.OleCtrls, uWVWinControl,
  uWVWindowParent, uWVBrowserBase, uWVBrowser, uWVTypeLibrary;

type
  TAtlasWebNavPage = class(TForm)
  public
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;
  private
    WebViewParent: TWVWindowParent;
    WebBrowser: TWVBrowser;
    procedure InitBrowser(Sender: TObject);
    procedure WebBrowserDOMContentLoaded(Sender: TObject;
      const aWebView: ICoreWebView2;
      const aArgs: ICoreWebView2DOMContentLoadedEventArgs);
  public
  end;

function NewAtlasNavPage(const AURL: String; AParent: TWinControl = nil): TAtlasWebNavPage;

implementation

{$R *.DFM}

{ TAtlasWebNavPage }

uses
  uWVLoader,
  SysUtils;

function NewAtlasNavPage(const AURL: String; AParent: TWinControl = nil): TAtlasWebNavPage;
begin
  Result := TAtlasWebNavPage.Create(AParent);
  try
    if (AParent <> nil) then
      Result.SetParent(AParent);
    Result.WebBrowser.DefaultURL := AURL;
    Result.OnShow := Result.InitBrowser;
  except
    FreeAndNil(Result);
    raise;
  end;
end;

constructor TAtlasWebNavPage.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  WebBrowser := TWVBrowser.Create(Self);
  WebBrowser.OnDOMContentLoaded := WebBrowserDOMContentLoaded;
  WebViewParent := TWVWindowParent.Create(Self);
  WebViewParent.Parent := Self;
  WebViewParent.Browser := WebBrowser;
end;

destructor TAtlasWebNavPage.Destroy;
begin

  inherited Destroy;
end;

procedure TAtlasWebNavPage.InitBrowser(Sender: TObject);
begin
  Self.OnShow := nil;
  GlobalWebView2Loader                    := TWVLoader.Create(nil);
  GlobalWebView2Loader.UseInternalLoader  := True;
  GlobalWebView2Loader.UserDataFolder     := ExtractFileDir(Application.ExeName) + '\CustomCache';
  GlobalWebView2Loader.StartWebView2;

  if not WebBrowser.CreateBrowser(WebViewParent.Handle) then
    RaiseLastOSError;
end;

procedure TAtlasWebNavPage.WebBrowserDOMContentLoaded(Sender: TObject;
  const aWebView: ICoreWebView2;
  const aArgs: ICoreWebView2DOMContentLoadedEventArgs);
begin
  WebViewParent.Align := alClient;
end;

end.
