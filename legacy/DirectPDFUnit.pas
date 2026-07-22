unit DirectPDFUnit;

interface

uses
  Classes, Windows, Graphics, ExtCtrls, SysUtils, Forms, Controls, ppFilDev, Math, RichEdit,
  ppDevice, ppTypes, ppUtils, ppForms, ppDrwCmd, JPEG, Dialogs, Printers, ComCtrls, ppViewr,
  ppRichTxDrwCmd, ppBarCodDrwCmd, 
  ppRichTx{$IFDEF VER130},ZLib{$ENDIF};

type

  TPDFOnReceiveStream = procedure (Sender: TObject; AStream: TStream) of Object;

  TDirectReportItemType = (riIgnore, riText, riImage, riLine, riShape, riRTF, riBarCode, riCheckBox);

  TImageCRC = class
    FileName: String;
    CRC: Cardinal;
  end;

  TReportBand = class(TList)
  end;

  TDirectReportItem = class
    ItemType: TDirectReportItemType;
    Row: Integer;
    Top: Integer;
    Left: Integer;
    Height: Integer;
    Width: Integer;
    AdjLeft: Integer;
    AdjHeight: Integer;
    AdjWidth: Integer;
    DrawCmd: TppDrawCommand;
    ZOrder: Integer;
  end;

  TppDirectDevice = class(TppFileDevice)
  private
    Page: TppPage;
    FRow: Integer;
    FCol: Integer;
    FPageNo: Integer;
    FImageNo: Integer;
    MemStream: TMemoryStream;
    SeparateBands: Boolean;
    ConvertFonts: Boolean;
    ImageList: TList;
    CRCTable: array[0..255] of Cardinal;
    FFileStream: TStream;
    procedure GetDrawCommands(Page: TppPage; Cmds: TStringList);
    procedure Write(Buffer: String); virtual;
    procedure SavePageToFile(Page: TppPage);
    procedure CalcSize(Itm: TDirectReportItem);
    procedure DrawCheckBox(B: TBitmap; Txt: TppDrawText; Bounds: TRect; AdjBitmap: Boolean; IgnoreAttr: Boolean);
    function  ImageIndex(J: TObject; FileName: String): Integer;
    function  CRC(MS: TMemoryStream): Cardinal;
    procedure InitCRCTable;
    procedure DrawImage(B: TBitmap; Img: TppDrawImage; Bounds: TRect; AdjBitmap: Boolean; IgnoreAttr: Boolean);
    procedure DrawBarCode(B: TCanvas; Bar: TppDrawBarCode; Bounds: TRect);
    function ConvertFont(FontName: String): String;
  protected
    procedure Stream(Buffer: String);
    procedure DrawLine(B: TCanvas; Lne: TppDrawLine; Bounds: TRect);
    procedure DrawShape(B: TCanvas; Shp: TppDrawShape; Bounds: TRect);
    function  WriteImage(B: TBitmap): String;
    procedure DrawRichText(B: TBitmap; DRT: TppDrawRichText; Bounds: TRect);
    procedure ProcessBand(Band: TReportBand); virtual;
    procedure StartBand; virtual;
    procedure EndBand; virtual;
    procedure StartPage; virtual;
    procedure EndPage; virtual;
  public
    constructor Create(aOwner: TComponent); override;
    destructor Destroy; override;
    procedure EndJob; override;
    procedure StartJob; override;
    procedure ReceivePage(aPage: TppPage); override;

    property FileStream: TStream read FFileStream;
  end;

  TDirectPDFDevice = class(TppDirectDevice)
  private
    FontTbl: TStringList;
    FontObj: TStringList;
    CrossRef: TStringList;
    ObjNo: Integer;
    RootObj: Integer;
    BodyObj: String;
    ProcObj: String;
    MaxKids: Integer;
    AnyImages: Boolean;
    InStream: Boolean;
    InImage: Boolean;
    StreamObj: String;
    PageList: TStringList;
    TotPages: Integer;
    StreamSize: Integer;
    PageHeight: Integer;
    PageWidth: Integer;
    FilePos: Integer;
    XRefPos: Integer;
    procedure AddRef(Obj: String);
    procedure WriteCrossRef;
    function  ImageObject(Itm: TDirectReportItem): String;
    procedure WriteBMP(ImgObj: String; B: TBitmap; Mask: Boolean);
    procedure ASCII85(Source, Target: TStream);
    procedure EndStream;
    procedure StartStream;
    procedure RunLength(Source, Target: TStream);
    function  ArcTo(X1, Y1, X2, Y2, XRadius, YRadius: Extended): String;
    procedure RoundRect(Left, Top, Width, Height, XRad, YRad: Integer);
  protected
    procedure StartBand; override;
    procedure EndBand; override;
    procedure StartPage; override;
    procedure EndPage; override;
    procedure ProcessBand(Band: TReportBand); override;
    procedure WriteFontTbl;
    function  FontIndex(Font: TFont): Integer;
    procedure Write(Buffer: String); override;
    function  NextObj: String;
    function  ParentObj(PageNo: Integer): String;
    function  FontFamily(Font: TFont): String;
  public
    procedure StartJob; override;
    procedure EndJob; override;
    constructor Create(aOwner: TComponent); override;
    destructor Destroy; override;
    class function DeviceName: String; override;
  end;

var
  PDFOnReceiveStream : TPDFOnReceiveStream = nil;

implementation

const
  CRLF = #13 + #10;
  EOP = #36 + #182;

function PixelsToPoints(Value: Extended): Integer;
begin
  Result := Trunc((Value * (1 / 96)) * 72);
end;

function TextHeight(Font: TFont): Integer;
var
  DC: HDC;
  TM: TTextMetric;
  SaveFont: HFont;
begin
  DC := GetDC(0);
  SaveFont := SelectObject(DC, Font.Handle);
  GetTextMetrics(DC, TM);
  Result := TM.tmAscent + TM.tmDescent + TM.tmExternalLeading;
  SelectObject(DC, SaveFont);
  ReleaseDC(0, DC);
  Result := PixelsToPoints(Result);
end;

function ThousandthsToHorzPixels(Value: Integer): Integer;
begin
  Result := ppToScreenPixels(Value, utMMThousandths, pprtHorizontal, Nil);
end;

function EThousandthsToPoints(Value: Integer): Extended;
begin
  Result := (Value / 1000) * 0.0393701 * 72;
end;

function Replace(cString, cSearch, cReplace: String): String;
var
  nSize, nPos, I, nOrig, nNext: Integer;
begin
  I      := 0;
  nPos   := 0;
  nSize  := Length(cReplace) - Length(cSearch);
  nOrig  := Length(cString);
  Result := cString;
  nNext  := Pos(cSearch, cString);

  while nNext > 0 do begin
    nPos := nPos + nNext;
    Delete(Result, nPos + (I * nSize), Length(cSearch));
    Insert(cReplace, Result, nPos + (I * nSize));
    Inc(I);
    nNext := Pos(cSearch, Copy(cString, nPos + 1, nOrig - nPos));
  end;
end;

function RTFToPlainString(RTF: TppDrawRichText): String;
var
  RE: TRichEdit;
  N: Integer;
  lFakeFrm: TForm;
begin
  RE := TRichEdit.Create(Nil);
  RE.Parent := TWinControl(RTF.Parent);
  if RE.Parent = nil then begin
    lFakeFrm := TForm.Create(nil);
    RE.Parent := lFakeFrm;
  end
  else lFakeFrm := nil;
  try
  RTF.RichTextStream.Position := 0;
  RE.Width := ThousandthsToHorzPixels(RTF.Width);
  RE.Lines.LoadFromStream(RTF.RichTextStream);

  RE.SelStart := 0;
  RE.SelLength := RTF.StartCharPos;
  RE.ClearSelection;

  RE.SelectAll;
  N := RE.SelLength;
  RE.SelStart := RTF.EndCharPos + 1;
  RE.SelLength := N;
  RE.ClearSelection;

  Result := RE.Lines.Text;
  finally
   RE.Free;
   lFakeFrm.Free;
  end;
end;

function FmtFloat(Value: Extended): String;
begin
  Result := FloatToStr(Value);
  Result := Replace(Result, DecimalSeparator, '.');
end;

function ColorToPDF(Value: TColor): String;
var
  Color: Integer;
begin
  Color  := ColorToRGB(Value);
  Result := FormatFloat('0.000', GetRValue(Color) / 255) + ' ' + FormatFloat('0.000', GetGValue(Color) / 255) + ' ' + FormatFloat('0.000', GetBValue(Color) / 255);
  Result := Replace(Result, DecimalSeparator, '.');
end;

function ThousandthsToPoints(Value: Integer): Integer;
begin
  Result := Round((Value / 1000) * 0.0393701 * 72);
end;

function ZOrderSort(Item1, Item2: Pointer): Integer;
begin
  Result := 0;
  if TDirectReportItem(Item1).ZOrder > TDirectReportItem(Item2).ZOrder then begin
     Result := 1;
  end;
  if TDirectReportItem(Item1).ZOrder < TDirectReportItem(Item2).ZOrder then begin
     Result := -1;
  end;
  if TDirectReportItem(Item1).ZOrder = TDirectReportItem(Item2).ZOrder then begin
     Result := 0;
  end;
end;

function ThousandthsToVertPixels(Value: Integer): Integer;
begin
  Result := ppToScreenPixels(Value, utMMThousandths, pprtVertical, Nil);
end;

function PointsToPixels(Value: Extended): Integer;
begin
  Result := Trunc((Value * (1 / 72)) * 96);
end;

function Occurs(Value, Sub: String): Integer;
var
  I: Integer;
begin
  Result := 0;
  for I := 1 to Length(Value) do begin
    if Value[I] = Sub then begin
       Result := Result + 1;
    end;
  end;
end;

function VertPixelsToThousandths(Value: Integer): Integer;
begin
  Result := Round(ppFromScreenPixels(Value, utMMThousandths, pprtVertical, Nil));
end;

{ TppDirectDevice }

procedure TppDirectDevice.CalcSize(Itm: TDirectReportItem);
var
  Bmp: TBitmap;
  Cmd: TppDrawText;
  Right, Center, Left, Width, Height: Integer;
begin
  Itm.AdjLeft   := Itm.Left;
  Itm.AdjWidth  := Itm.Width;
  Itm.AdjHeight := Itm.Height;

  if Itm.DrawCmd is TppDrawText then begin
     Left   := Itm.Left;
     Width  := Itm.Width;
     Height := Itm.Height;
     Center := Itm.Left + Itm.Width div 2;
     Right  := Itm.Left + Itm.Width;
     Cmd := TppDrawText(Itm.DrawCmd);

     Bmp := TBitmap.Create;
     Bmp.Canvas.Font := Cmd.Font;
     if Cmd.IsMemo then begin
     end else begin
        Width  := Bmp.Canvas.TextWidth(Cmd.Text);
        Width  := Trunc(ppFromScreenPixels(Width, utMMThousandths, pprtHorizontal, Nil));
        Height := Bmp.Canvas.TextHeight(Cmd.Text);
        Height := Trunc(ppFromScreenPixels(Height, utMMThousandths, pprtVertical, Nil));
     end;
     if Cmd.TextAlignment = taRightJustified then begin
        Left := Right - Width; // - 2000;   {1/8/00} Removed
     end;
     if Cmd.TextAlignment = taCentered then begin
        Left := Center - Width div 2;
     end;
     Bmp.Free;

     if Cmd.AutoSize = True then begin
        Itm.Left   := Left;
        Itm.Width  := Width;
        Itm.Height := Height;
     end;
     Itm.AdjLeft   := Left;
     Itm.AdjWidth  := Width;
     Itm.AdjHeight := Height;
  end;
end;

function TppDirectDevice.ConvertFont(FontName: String): String;
begin
  Result := 'Times New Roman';

  if Pos('COURIER', UpperCase(FontName)) <> 0 then begin
     Result := 'Courier';
  end;

  if Pos('ARIAL', UpperCase(FontName)) <> 0 then begin
     Result := 'Arial';
  end;

  if Pos('WINGDINGS', UpperCase(FontName)) <> 0 then begin
     Result := 'Wingdings';
  end;

  if Pos('TIMES', UpperCase(FontName)) <> 0 then begin
     Result := 'Times New Roman';
  end;
end;

function TppDirectDevice.CRC(MS: TMemoryStream): Cardinal;
var
  I, J: Integer;
  B: String;
begin
  Result := 0;
  MS.Position := 0;
  B := ' ';
  for I := 0 to MS.Size - 1 do begin
    MS.Read(B[1], 1);
    J := (Ord(B[1]) xor Result) and $000000FF;
    Result := (Result shr 8) xor CRCTable[J];
  end;
  Result := not Result;
end;

constructor TppDirectDevice.Create(aOwner: TComponent);
begin
  inherited;
  FFileStream := TMemoryStream.Create;
  MemStream := TMemoryStream.Create;
  SeparateBands := True;
  ConvertFonts := False;
  InitCRCTable;
  ImageList := TList.Create;
end;

destructor TppDirectDevice.Destroy;
var
  I: Integer;
begin
  MemStream.Free;
  for I := 0 to ImageList.Count - 1 do begin
    TImageCRC(ImageList[I]).Free;
  end;
  ImageList.Free;
  inherited;
end;

procedure TppDirectDevice.DrawBarCode(B: TCanvas; Bar: TppDrawBarCode;
  Bounds: TRect);
var
  T: TBitmap;
  P: TPoint;
begin

  T := TBitmap.Create;

  Bar.CalcBarCodeSize(T.Canvas);

  if Bar.Orientation in [orLeftToRight, orRightToLeft] then begin
     T.Width  := Bar.PortraitWidth;
     T.Height := Bar.PortraitHeight;
  end else begin
     T.Width  := Bar.PortraitHeight;
     T.Height := Bar.PortraitWidth;
  end;

  P := Point(Screen.PixelsPerInch, Screen.PixelsPerInch);
  T.Canvas.Pen.Color := clBlack;

  Bar.DrawBarCode(T.Canvas, 0, 0, P, True);
  B.StretchDraw(Bounds, T);
  T.Free;

end;

procedure TppDirectDevice.DrawCheckBox(B: TBitmap; Txt: TppDrawText;
  Bounds: TRect; AdjBitmap, IgnoreAttr: Boolean);
var
  Width, Height: Integer;
begin
  B.Canvas.Font := Txt.Font;
  B.Canvas.Font.Size := B.Canvas.Font.Size - Trunc(B.Canvas.Font.Size * 0.10);
  Width  := Bounds.Right - Bounds.Left;
  Height := Bounds.Bottom - Bounds.Top;

  if AdjBitmap then begin
     B.Width  := Width;
     B.Height := Height;
     B.PixelFormat := pf24bit;
  end;

  B.Canvas.TextOut(0, 1, Txt.Text);
end;

procedure TppDirectDevice.DrawImage(B: TBitmap; Img: TppDrawImage;
  Bounds: TRect; AdjBitmap, IgnoreAttr: Boolean);
var
  Scale: Extended;
  R: TRect;
  W: TBitmap;
  Width, Height: Integer;
begin

  Width  := Bounds.Right - Bounds.Left;
  Height := Bounds.Bottom - Bounds.Top;

  W := TBitmap.Create;
  W.Width  := Width;
  W.Height := Height;
  W.PixelFormat := B.PixelFormat;

  if not IgnoreAttr then begin
     if Img.Stretch and Img.MaintainAspectRatio then begin
        R := Rect(0, 0, Width, Height);
        Scale := ppCalcAspectRatio(Img.Picture.Width, Img.Picture.Height, Width, Height);
        if Img.Center then begin
           R.Left := R.Left + ((Width  - Trunc(Img.Picture.Width  * Scale)) div 2);
           R.Top  := R.Top  + ((Height - Trunc(Img.Picture.Height * Scale)) div 2);
        end;
        R.Right  := R.Left + Trunc(Img.Picture.Width  * Scale);
        R.Bottom := R.Top  + Trunc(Img.Picture.Height * Scale);
     end else if Img.Stretch then begin
        R := Rect(0, 0, Width, Height);
     end else if Img.Center then begin
        R := Rect((Width - Img.Picture.Width) div 2, (Height - Img.Picture.Height) div 2, Img.Picture.Width, Img.Picture.Height)
     end else begin
        R := Rect(0, 0, Img.Picture.Width, Img.Picture.Height);
     end;
  end else begin
     R := Rect(0, 0, Img.Picture.Width, Img.Picture.Height);
  end;

  if AdjBitmap then begin
     B.Width  := Width;
     B.Height := Height;
     B.PixelFormat := W.PixelFormat;

     if Img.Picture.Graphic is TMetaFile then begin
        B.Palette := Img.Picture.MetaFile.Palette;
     end;
     if Img.Picture.Graphic is TBitmap then begin
        B.Palette := Img.Picture.Bitmap.Palette;
     end;
  end;

  W.Canvas.StretchDraw(R, Img.Picture.Graphic);
  B.Canvas.CopyMode := cmSrcCopy;
  B.Canvas.CopyRect(Bounds, W.Canvas, Rect(0, 0, Width, Height));

  W.Free;

end;

procedure TppDirectDevice.DrawLine(B: TCanvas; Lne: TppDrawLine;
  Bounds: TRect);
var
  Width, Height, N, L, H, X, XOffset, YOffset: Integer;
begin

  Width  := Bounds.Right - Bounds.Left;
  Height := Bounds.Bottom - Bounds.Top;
  B.Pen.Assign(Lne.Pen);
  B.Pen.Width := 1;

  X := PointsToPixels(Lne.Weight);
  if X = 0 then begin
     X := 1;
  end;

  if Lne.LineStyle = lsSingle then begin
     N := 1;
  end else begin
     N := 2;
  end;

  XOffset := Bounds.Left;
  YOffset := Bounds.Top;

  for L := 0 to N - 1 do begin

    for H := 0 to X - 1 do begin

      if Lne.LinePosition = lpTop then begin
         B.MoveTo(XOffset, YOffset + H + (L * X * 2));
         B.LineTo(XOffset + Width, YOffset + H + (L * X * 2));
      end;

      if Lne.LinePosition = lpBottom then begin
         B.MoveTo(XOffset, YOffset - H - (L * X * 2));
         B.LineTo(XOffset + Width, YOffset - H - (L * X * 2));
      end;

      if Lne.LinePosition = lpLeft then begin
         B.MoveTo(XOffset + H + (L * X * 2), YOffset);
         B.LineTo(XOffset + H + (L * X * 2), YOffset + Height);
      end;

      if Lne.LinePosition = lpRight then begin
         B.MoveTo(XOffset + Width - H - (L * X * 2) - 1, YOffset);
         B.LineTo(XOffset + Width - H - (L * X * 2) - 1, YOffset + Height);
      end;

    end;

  end;

end;

procedure TppDirectDevice.DrawRichText(B: TBitmap; DRT: TppDrawRichText;
  Bounds: TRect);
var
  MF: TMetaFile;
  MC: TMetaFileCanvas;
  Width, Height: Integer;
  CharRange: TCharRange;
  DC: hDC;
  R: TRect;
  RE: TCustomRichEdit;
  ppParentWnd: TWinControl;
begin
  ppParentWnd := TWinControl(DRT.Parent);
  RE := ppGetRichEditClass.Create(ppParentWnd);
  RE.Parent := ppParentWnd;

  DRT.RichTextStream.Position := 0;
  ppGetRichEditLines(RE).LoadFromStream(DRT.RichTextStream);

  CharRange.cpMin := DRT.StartCharPos;
  CharRange.cpMax := DRT.EndCharPos;

  DC := GetDC(0);

  Width  := Bounds.Right - Bounds.Left;
  Height := Bounds.Bottom - Bounds.Top;

  R := Rect(0, 0, Width, Height);

  MF := TMetaFile.Create;
  MF.Width  := Width;
  MF.Height := Height;

  MC := TMetaFileCanvas.Create(MF, DC);

  if not DRT.Transparent then begin
     MC.Brush.Style := bsSolid;
     MC.Brush.Color := DRT.Color;
     MC.FillRect(Bounds);
  end;

  //ppGetRTFEngine(RE).DrawRichText(MC.Handle, DC, R, CharRange);
// RE.DrawRichText(MC.Handle, DC, R, CharRange);
  TppRTFEngine.DrawRichText(RE,MC.Handle, DC, R, CharRange);

  MC.Free;
  ReleaseDC(0, DC);

  B.Canvas.StretchDraw(Bounds, MF);

  MF.Free;
  RE.Free;

end;

procedure TppDirectDevice.DrawShape(B: TCanvas; Shp: TppDrawShape;
  Bounds: TRect);
var
  Top, Left, Width, Height, XCR, YCR: Integer;
begin

//  Removed 4/9/00
//  if Shp.Pen.Width <> 1 then begin
//     InflateRect(Bounds, -Shp.Pen.Width, -Shp.Pen.Width);
//  end;

  Width  := Bounds.Right - Bounds.Left;
  Height := Bounds.Bottom - Bounds.Top;

  B.Brush.Assign(Shp.Brush);
  B.Pen.Assign(Shp.Pen);

  if Shp.ShapeType in [stCircle] then begin
     if Width > Height then begin
        Left := Bounds.Left + (Width - Height) div 2;
        B.Ellipse(Left, Bounds.Top, Left + Height, Bounds.Bottom);
     end else begin
        Top := Bounds.Top + (Height - Width) div 2;
        B.Ellipse(Bounds.Left, Top, Bounds.Right, Top + Width);
     end;
  end;

  if Shp.ShapeType in [stEllipse] then begin
     B.Ellipse(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom);
  end;

  if Shp.ShapeType in [stSquare] then begin
     if Width > Height then begin
        Left := Bounds.Left + (Width - Height) div 2;
        B.Rectangle(Left, Bounds.Top, Left + Height, Bounds.Bottom);
     end else begin
        Top := Bounds.Top + (Height - Width) div 2;
        B.Rectangle(Bounds.Left, Top, Bounds.Right, Top + Width);
     end;
  end;

  if Shp.ShapeType in [stRectangle] then begin
     B.Rectangle(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom);
  end;

  if Shp.ShapeType in [stRoundSquare] then begin
     XCR := ppToScreenPixels(Shp.XCornerRound, utMMThousandths, pprtHorizontal, Nil);
     YCR := ppToScreenPixels(Shp.YCornerRound, utMMThousandths, pprtVertical, Nil);
     if Width > Height then begin
        Left := Bounds.Left + (Width - Height) div 2;
        B.RoundRect(Left, Bounds.Top, Left + Height, Bounds.Bottom, XCR, YCR);
     end else begin
        Top := Bounds.Top + (Height - Width) div 2;
        B.RoundRect(Bounds.Left, Top, Bounds.Right, Top + Width, XCR, YCR);
     end;
  end;

  if Shp.ShapeType in [stRoundRect] then begin
     XCR := ppToScreenPixels(Shp.XCornerRound, utMMThousandths, pprtHorizontal, Nil);
     YCR := ppToScreenPixels(Shp.YCornerRound, utMMThousandths, pprtVertical, Nil);
     B.RoundRect(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom, XCR, YCR);
  end;

end;

procedure TppDirectDevice.EndBand;
begin
  Inc(FRow);
end;

procedure TppDirectDevice.EndJob;
begin
  inherited;
end;

procedure TppDirectDevice.EndPage;
begin
end;

procedure TppDirectDevice.GetDrawCommands(Page: TppPage;
  Cmds: TStringList);
var
  I, N, W, X, Y, Row, LastTop: Integer;
  DrawCmd: TppDrawCommand;
  Order: String;
  Itm: TDirectReportItem;
  Txt: TppDrawText;
begin
  N := Page.DrawCommandCount;

  for I := 0 to N - 1 do begin
    DrawCmd := Page.DrawCommands[I];
    Order := '';

    Itm := TDirectReportItem.Create;
    Itm.ItemType := riIgnore;
    Itm.DrawCmd  := DrawCmd;
    Itm.Top      := DrawCmd.Top;
    Itm.Left     := DrawCmd.Left;
    Itm.Width    := DrawCmd.Width;
    Itm.Height   := DrawCmd.Height;
    Itm.ZOrder   := I;

    if DrawCmd is TppDrawLine then begin
       Itm.ItemType := riLine;
       if TppDrawLine(DrawCmd).LineStyle = lsSingle then begin
          X := 1;
       end else begin
          X := 2;
       end;
       W := VertPixelsToThousandths(Trunc(TppDrawLine(DrawCmd).Pen.Width));
       case TppDrawLine(DrawCmd).LinePosition of
         lpTop:    begin
                     Itm.Height := X * (W + 3);
                   end;
         lpBottom: begin
                     Y := X * (W + 3);
                     Itm.Top := Itm.Top + Itm.Height - Y;
                     Itm.Height := Y;
                   end;
         lpLeft:   begin
                     Itm.Width := X * (W + 3);
                   end;
         lpRight:  begin
                     Y := X * (W + 3);
                     Itm.Left := Itm.Left + Itm.Width - Y;
                     Itm.Width := Y;
                     Itm.AdjWidth := Itm.Width;
                   end;
       end;
    end;

    if DrawCmd is TppDrawText then begin
       Txt := TppDrawText(Itm.DrawCmd);
       Itm.ItemType := riText;
       if (UpperCase(Txt.Font.Name) = 'WINGDINGS') and (Length(Txt.Text) = 1) then begin
          if (Txt.Text[1] in [#168, #254, #252, #251, #253]) then begin
             Itm.ItemType := riCheckBox;
          end;
       end;
       TppDrawText(Itm.DrawCmd).Font.Size := Abs(TppDrawText(Itm.DrawCmd).Font.Size);
       if ConvertFonts then begin
          TppDrawText(Itm.DrawCmd).Font.Name := ConvertFont(TppDrawText(Itm.DrawCmd).Font.Name);
       end;
    end;

    if DrawCmd is TppDrawRichText then begin
       Itm.ItemType := riRTF;
    end;

    if DrawCmd is TppDrawImage then begin
       Itm.ItemType := riImage;
    end;

    if DrawCmd is TppDrawShape then begin
       Itm.ItemType := riShape;
    end;

    if DrawCmd is TppDrawBarCode then begin
       Itm.ItemType := riBarCode;
    end;

    CalcSize(Itm);

    Order := FormatFloat('00000000', Itm.Top) + FormatFloat('00000000', Itm.Left);

    Cmds.AddObject(Order, Itm);
  end;

  Cmds.Sort;
  Row := 0;
  I   := 0;

  while I < Cmds.Count do begin

    Itm := TDirectReportItem(Cmds.Objects[I]);
    LastTop := Itm.Top + 2000;

    while LastTop > Itm.Top do begin
      Itm.Row := Row;
      Cmds[I] := FormatFloat('00000000', Itm.Row) + FormatFloat('00000000', Itm.Left);
      Inc(I);

      if I >= Cmds.Count then begin
         Break;
      end else begin
         Itm := TDirectReportItem(Cmds.Objects[I]);
      end;
    end;

    Inc(Row);
  end;

  Cmds.Sort;

end;

function TppDirectDevice.ImageIndex(J: TObject; FileName: String): Integer;
var
  MS: TMemoryStream;
  I: Integer;
  N: Cardinal;
  T: TImageCRC;
begin
  MS := TMemoryStream.Create;
  if J is TJPEGImage then begin
     (J as TJPEGImage).SaveToStream(MS);
  end else begin
     (J as TBitmap).SaveToStream(MS);
  end;
  Result := -1;
  N := CRC(MS);
  for I := 0 to ImageList.Count - 1 do begin
    if TImageCRC(ImageList[I]).CRC = N then begin
       Result := I;
       Break;
    end;
  end;

  if Result = -1 then begin
     T := TImageCRC.Create;
     T.FileName := FileName;
     T.CRC := N;
     ImageList.Add(T);
  end;

  MS.Free;
end;

procedure TppDirectDevice.InitCRCTable;
var
  I, J: Integer;
begin
  for I := 0 to 255 do begin
    CRCTable[I] := I;
    for J := 0 to 7 do begin
      if Odd(CRCTable[I]) then begin
         CRCTable[I] := CRCTable[I] shr 1;
         CRCTable[I] := CRCTable[I] xor $EDB88320;
      end else begin
         CRCTable[I] := CRCTable[I] shr 1;
      end;
    end;
  end;
end;

procedure TppDirectDevice.ProcessBand(Band: TReportBand);
begin
end;

procedure TppDirectDevice.ReceivePage(aPage: TppPage);
begin
  inherited;
  Page := aPage;

  if IsRequestedPage then begin
     if not IsMessagePage then begin
        SavePageToFile(aPage);
     end;
  end;
end;

procedure TppDirectDevice.SavePageToFile(Page: TppPage);
var
  I: Integer;
  Cmds: TStringList;
  RptItem: TDirectReportItem;
  LastRow: Integer;
  Band: TReportBand;
begin

  Cmds := TStringList.Create;
  GetDrawCommands(Page, Cmds);

  if Cmds.Count = 0 then begin
     Cmds.Free;
     Exit;
  end;

  StartPage;

  // Process Commands

  I := 0;

  while I < Cmds.Count do begin

    RptItem := TDirectReportItem(Cmds.Objects[I]);
    LastRow := RptItem.Row;

    StartBand;

    Band := TReportBand.Create;

    while (not SeparateBands) or (LastRow = RptItem.Row) do begin
      Band.Add(RptItem);
      Inc(I);

      if I >= Cmds.Count then begin
         Break;
      end else begin
         RptItem := TDirectReportItem(Cmds.Objects[I]);
      end;
    end;

    ProcessBand(Band);
    Band.Free;

    EndBand;

  end;

  EndPage;

  for I := 0 to Cmds.Count - 1 do begin
    TDirectReportItem(Cmds.Objects[I]).Free;
  end;

  Cmds.Free;

end;

procedure TppDirectDevice.StartBand;
begin
  FCol := 0;
end;

procedure TppDirectDevice.StartJob;
begin
  inherited;
  FRow := 0;
  FCol := 0;
  FPageNo := 0;
  FImageNo := 0;
end;

procedure TppDirectDevice.StartPage;
begin
  Inc(FPageNo);
end;

procedure TppDirectDevice.Stream(Buffer: String);
begin
  if Length(Buffer) > 0 then begin
     MemStream.Write(Buffer[1], Length(Buffer));
  end;
end;

procedure TppDirectDevice.Write(Buffer: String);
begin
  if Length(Buffer) > 0 then begin
     FileStream.Write(Buffer[1], Length(Buffer));
  end;
end;

function TppDirectDevice.WriteImage(B: TBitmap): String;
var
  N: Integer;
  J: TJPEGImage;
begin
  Result := 'IMG' + FormatFloat('0000', FImageNo) + '.JPG';

  J := TJPEGImage.Create;
  J.Assign(B);

  N := ImageIndex(J, Result);

  if N = -1 then begin
     Inc(FImageNo);
     try
       J.SaveToStream(FFileStream);
     except
       raise EPrintError.Create('File Error: ' + Result);
     end;
  end else begin
     Result := TImageCRC(ImageList[N]).FileName;
  end;
  J.Free;
end;

{ TDirectPDFDevice }

procedure TDirectPDFDevice.AddRef(Obj: String);
begin
  CrossRef.Add(FormatFloat('000000', StrToInt(Obj)) + ' ' + FormatFloat('0000000000', FilePos));
end;

function TDirectPDFDevice.ArcTo(X1, Y1, X2, Y2, XRadius,
  YRadius: Extended): String;
var
  C: array[1..6] of Extended;
  I: Integer;
  W, Y: Extended;
begin
  Result := '';
  C[5] := X2;
  C[6] := Y2;
  W := XRadius * 0.5523;
  Y := YRadius * 0.5523;

  if X2 > X1 then begin
     if Y2 > Y1 then begin
        C[1] := X1;
        C[2] := Y1 + Y;
        C[3] := X2 - W;
        C[4] := Y2;
     end else begin
        C[1] := X1 + W;
        C[2] := Y1;
        C[3] := X2;
        C[4] := Y2 + Y;
     end;
  end else begin
     if Y2 > Y1 then begin
        C[1] := X1 - W;
        C[2] := Y1;
        C[3] := X2;
        C[4] := Y2 - Y;
     end else begin
        C[1] := X1;
        C[2] := Y1 - Y;
        C[3] := X2 + W;
        C[4] := Y2;
     end;
  end;

  for I := 1 to 6 do begin
    Result := Result + FormatFloat('###0.000', C[I]) + ' ';
  end;

  Result := Replace(Result, DecimalSeparator, '.');
  Result := Result + 'c';
end;

procedure TDirectPDFDevice.ASCII85(Source, Target: TStream);
var
  Bytes: Integer;
  I: Integer;
  Total: Cardinal;
  InBuffer: array[0..3] of Byte;
  OutBuffer: array[0..4] of Byte;
begin

  Source.Position := 0;
  Target.Position := 0;

  while Source.Position < Source.Size do begin

    for I := 0 to High(InBuffer) do begin
      InBuffer[I] := 0;
    end;

    for I := 0 to High(OutBuffer) do begin
      OutBuffer[I] := 0;
    end;

    Bytes := Source.Read(InBuffer, 4);

    Total := 0;
    for I := 0 to High(InBuffer) do begin
      Total := Total + (InBuffer[I] * Trunc(IntPower(256, 3 - I)));
    end;

    if (Total = 0) and (Bytes = 4) then begin
       OutBuffer[0] := 122;
       Target.Write(OutBuffer, 1);
    end else begin
       for I := 0 to High(OutBuffer) do begin
         OutBuffer[I] := Trunc(Total / IntPower(85, 4 - I));
         Total := Total - (OutBuffer[I] * Trunc(IntPower(85, 4 - I)));
         OutBuffer[I] := OutBuffer[I] + 33;
       end;

       Target.Write(OutBuffer, Bytes + 1);

    end;

  end;

  OutBuffer[0] := Ord('~');
  OutBuffer[1] := Ord('>');
  Target.Write(OutBuffer, 2);

  Source.Position := 0;
  Target.Position := 0;

end;

constructor TDirectPDFDevice.Create(aOwner: TComponent);
begin
  inherited;
  PageList := TStringList.Create;
  FontTbl  := TStringList.Create;
  FontObj  := TStringList.Create;
  CrossRef := TStringList.Create;
  ConvertFonts := True;
end;

destructor TDirectPDFDevice.Destroy;
begin
  PageList.Free;
  FontTbl.Free;
  FontObj.Free;
  CrossRef.Free;
  inherited;
end;

class function TDirectPDFDevice.DeviceName: String;
begin
  Result := 'DirectPDFToFile';
end;

procedure TDirectPDFDevice.EndBand;
begin
  inherited;
end;

procedure TDirectPDFDevice.EndJob;
var
  I: Integer;
  Obj, Buffer: String;
begin

  WriteFontTbl;

  AddRef('2');

  Write('2 0 obj');
  Write('<< /Creator (ATLAS)');
  Write('/CreationDate (D:' + FormatDateTime('yyyymmdd', Date()) + ')');
  Write('/Title (Raport Generat prin ATLAS)');
  Write('/Author (Advanced Technology Systems)');
  Write('/Producer (ATLAS)');
  Write('/Keywords (ATLAS, ATS)');
  Write('/Subject (Raport Generat Automat) >>');
  Write('endobj');

  AddRef('3');

  // Main Pages Root

  Write('3 0 obj');
  Write('<< /Type /Pages ');
  Buffer := '/Kids [ ';
  for I := 0 to PageList.Count - 1 do begin
    Buffer := Buffer + Trim(Copy(PageList[I], 1, 6)) + ' 0 R ';
  end;
  Buffer := Buffer + ' ] ';
  Write(Buffer);
  Write('/Count ' + IntToStr(TotPages) + ' >> ');
  Write('endobj');

  for I := 0 to PageList.Count - 1 do begin
    Obj := Trim(Copy(PageList[I], 1, 6));
    AddRef(Obj);
    Write(Obj + ' 0 obj');
    Write('<< /Type /Pages ');
    Write('/Kids [ ' + Copy(PageList[I], 7, Length(PageList[I]) - 7) + ' ] ');
    Write('/Count ' + IntToStr(Occurs(PageList[I], 'R')) + ' ');
    Write('/Parent 3 0 R >> ');
    Write('endobj');
  end;

  WriteCrossRef;

  Write('trailer');
  Write('<< /Root 1 0 R ');
  Write('/Info 2 0 R ');
  Write('/Size ' + IntToStr(CrossRef.Count + 1) + ' >> ');
  Write('startxref');
  Write(IntToStr(XRefPos));
  inherited Write('%%EOF');

  inherited;

  if Assigned(PDFOnReceiveStream) then PDFOnReceiveStream(Self, FFileStream);
end;

procedure TDirectPDFDevice.EndPage;
var
  StreamLen: String;
begin
  inherited;

  EndStream;
  StreamLen := IntToStr(StreamSize - 1);
  Write('endstream');
  Write('endobj');
  AddRef(StreamObj);
  Write(StreamObj + ' 0 obj');
  Write(StreamLen);
  Write('endobj');

  AddRef(ProcObj);
  Write(ProcObj + ' 0 obj');
  if AnyImages then begin
     Write('[ /PDF /Text /ImageC ] ');
  end else begin
     Write('[ /PDF /Text ] ');
  end;
  Write('endobj');
end;

procedure TDirectPDFDevice.EndStream;
var
  OutStream, TS: TMemoryStream;
  {$IFDEF VER130}
  CS: TCompressionStream;
  {$ENDIF}
  B: String;
  N: Integer;
begin
  InStream := False;
  OutStream := TMemoryStream.Create;
  MemStream.Position := 0;

  {$IFDEF VER130}
  TS := TMemoryStream.Create;
  CS := TCompressionStream.Create(clMax, TS);
  CS.CopyFrom(MemStream, 0);
  CS.Free;
  TS.Position := 0;
  ASCII85(TS, OutStream);
  TS.Free;
  {$ELSE}
  if InImage then begin
     TS := TMemoryStream.Create;
     RunLength(MemStream, TS);
     ASCII85(TS, OutStream);
     TS.Free;
  end else begin
     ASCII85(MemStream, OutStream);
  end;
  {$ENDIF}

  OutStream.Position := 0;
  B := StringOfChar(' ', 80);
  while OutStream.Position < OutStream.Size do begin
    N := 80;
    if N > (OutStream.Size - OutStream.Position) then begin
       N := (OutStream.Size - OutStream.Position);
    end;
    OutStream.Read(B[1], N);
    Write(Copy(B, 1, N));
  end;

  MemStream.Clear;
  OutStream.Free;
end;

function TDirectPDFDevice.FontFamily(Font: TFont): String;
begin
  Result := 'Times-Roman';

  if Pos('COURIER', UpperCase(Font.Name)) <> 0 then begin
     if fsBold in Font.Style then begin
        if fsItalic in Font.Style then begin
           Result := 'Courier-BoldOblique';
        end else begin
           Result := 'Courier-Bold';
        end;
     end else if fsItalic in Font.Style then begin
        Result := 'Courier-Oblique';
     end else begin
        Result := 'Courier';
     end;
  end;

  if Pos('ARIAL', UpperCase(Font.Name)) <> 0 then begin
     if fsBold in Font.Style then begin
        if fsItalic in Font.Style then begin
           Result := 'Helvetica-BoldOblique';
        end else begin
           Result := 'Helvetica-Bold';
        end;
     end else if fsItalic in Font.Style then begin
        Result := 'Helvetica-Oblique';
     end else begin
        Result := 'Helvetica';
     end;
  end;

  if Pos('WINGDINGS', UpperCase(Font.Name)) <> 0 then begin
     Result := 'ZapfDingbats';
  end;

  if (Pos('TIMES', UpperCase(Font.Name)) <> 0) or (Result = 'Times-Roman') then begin
     if fsBold in Font.Style then begin
        if fsItalic in Font.Style then begin
           Result := 'Times-BoldItalic';
        end else begin
           Result := 'Times-Bold';
        end;
     end else if fsItalic in Font.Style then begin
        Result := 'Times-Italic';
     end else begin
        Result := 'Times-Roman';
     end;
  end;

end;

function TDirectPDFDevice.FontIndex(Font: TFont): Integer;
var
  I: Integer;
  FontStr: String;
begin
  FontStr := FontFamily(Font);
  I := FontTbl.IndexOf(FontStr);
  if I = -1 then begin
     FontTbl.Add(FontStr);
     FontObj.Add(NextObj());
     I := FontTbl.Count - 1;
  end;
  Result := I;
end;

function TDirectPDFDevice.ImageObject(Itm: TDirectReportItem): String;
var
  B: TBitmap;
  N: Integer;
  Mask: Boolean;
begin
  B := TBitmap.Create;
  B.PixelFormat := pf24Bit;
  Mask := False;

  if Itm.ItemType = riImage then begin
     DrawImage(B, TppDrawImage(Itm.DrawCmd), Rect(0, 0, ThousandthsToHorzPixels(Itm.Width), ThousandthsToVertPixels(Itm.Height)), True, False);
  end;

  if Itm.ItemType = riCheckBox then begin
     Mask := True;
     DrawCheckBox(B, TppDrawText(Itm.DrawCmd), Rect(0, 0, ThousandthsToHorzPixels(Itm.Width), ThousandthsToVertPixels(Itm.Height)), True, False);
     B.PixelFormat := pf1Bit;
  end;

  if Itm.ItemType = riBarCode then begin
     B.Canvas.Pen.Color := clBlack;
     B.Canvas.Brush.Style := bsClear;
     B.Width  := ThousandthsToHorzPixels(Itm.Width);
     B.Height := ThousandthsToVertPixels(Itm.Height);
     DrawBarCode(B.Canvas, TppDrawBarCode(Itm.DrawCmd), Rect(0, 0, B.Width, B.Height));
  end;

  N := ImageIndex(B, '');

  if N = -1 then begin
     Result := NextObj();
     TImageCRC(ImageList[ImageList.Count - 1]).FileName := Result;
     WriteBMP(Result, B, Mask);
  end else begin
     Result := TImageCRC(ImageList[N]).FileName;
  end;
  B.Free;
end;

function TDirectPDFDevice.NextObj: String;
begin
  Result := IntToStr(ObjNo);
  Inc(ObjNo);
end;

function TDirectPDFDevice.ParentObj(PageNo: Integer): String;
begin
  Result := IntToStr(RootObj + (PageNo div MaxKids));
end;

procedure TDirectPDFDevice.ProcessBand(Band: TReportBand);
var
  I, L, N, Left, Top, Width, Height, TrueTop: Integer;
  Weight, TopMargin, CurImg, NewHeight: Integer;
  XRadius, YRadius, XOffset, YOffset, ELeft, ETop, EWidth, EHeight: Extended;
  Itm: TDirectReportItem;
  Txt: TppDrawText;
  Lne: TppDrawLine;
  Shp: TppDrawShape;
  ImgObj: TStringList;
  Buffer, ResObj: String;
  Font: TFont;
begin
  inherited;

  BodyObj   := NextObj();
  StreamObj := NextObj();
  ProcObj   := NextObj();
  ResObj    := NextObj();

  Band.Sort(ZOrderSort);

  //
  // Find all fonts on page
  //

  for I := 0 to Band.Count - 1 do begin
    Itm := TDirectReportItem(Band[I]);
    if Itm.ItemType = riText then begin
       FontIndex(TppDrawText(Itm.DrawCmd).Font);
    end;
    if Itm.ItemType = riRTF then begin
       Font := TFont.Create;
       Font.Name := 'Arial';
       FontIndex(Font);
       Font.Free;
    end;
  end;

  Write('/MediaBox [0 0 ' + IntToStr(PageWidth) + ' ' + IntToStr(PageHeight) + '] ');

  Write('/Resources ' + ResObj + ' 0 R');
  Write('/Contents ' + BodyObj + ' 0 R >> ');
  Write('endobj');

  //
  // Create all images
  //

  AnyImages := False;
  ImgObj := TStringList.Create;

  for I := 0 to Band.Count - 1 do begin
    Itm := TDirectReportItem(Band[I]);
    if Itm.ItemType in [riImage, riBarCode, riCheckBox] then begin
       AnyImages := True;
       ImgObj.Add(ImageObject(Itm));
    end;
  end;

  AddRef(ResObj);

  Write(ResObj + ' 0 obj');

  Write('<<');
  if FontObj.Count > 0 then begin
     Buffer := '/Font << ';
     for I := 0 to FontObj.Count - 1 do begin
       Buffer := Buffer + '/F' + IntToStr(I) + ' ' + FontObj[I] + ' 0 R ';
     end;
     Write(Buffer + '>>');
  end;

  if ImgObj.Count > 0 then begin
     Buffer := '/XObject << ';
     for I := 0 to ImgObj.Count - 1 do begin
       Buffer := Buffer + '/Im' + ImgObj[I] + ' ' + ImgObj[I] + ' 0 R ';
     end;
     Write(Buffer + '>>');
  end;

  Write('/ProcSet ' + ProcObj + ' 0 R');
  Write('>>');

  Write('endobj');

  AddRef(BodyObj);

  Write(BodyObj + ' 0 obj');
  {$IFDEF VER130}
  Write('<< /Length ' + StreamObj + ' 0 R /Filter [ /ASCII85Decode /FlateDecode ] >> ');
  {$ELSE}
  Write('<< /Length ' + StreamObj + ' 0 R /Filter /ASCII85Decode >> ');
  {$ENDIF}

  Write('stream');
  StartStream;

  CurImg := 0;

  TopMargin := 0;

//  RB includes top Margin in items
//  TopMargin := ThousandthsToPoints(Page.PageDef.mmMarginTop);

  for I := 0 to Band.Count - 1 do begin

    Itm := TDirectReportItem(Band[I]);

    Left   := ThousandthsToPoints(Itm.Left);
    Width  := ThousandthsToPoints(Itm.Width);
    Height := ThousandthsToPoints(Itm.Height);
    Top    := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin;

    // Process images & barcodes

    if Itm.ItemType in [riImage, riBarCode, riCheckBox] then begin
       Write('q');
       Write(ColorToPDF(clBlack) + ' RG');
       Write(IntToStr(Width) + ' 0 0 ' + IntToStr(Height) + ' ' + IntToStr(Left) + ' ' + IntToStr(Top - Height) + ' cm');
       Write('/Im' + ImgObj[CurImg] + ' Do');
       Write('Q');
       CurImg := CurImg + 1;
    end;

    // Process shapes

    if Itm.ItemType = riShape then begin

       Shp := TppDrawShape(Itm.DrawCmd);

       Write('q');

       if Shp.Pen.Width <> 1 then begin
          Write(IntToStr(PixelsToPoints(Shp.Pen.Width)) + ' w');
       end;

       if Shp.Pen.Color <> clBlack then begin
          Write(ColorToPDF(Shp.Pen.Color) + ' RG');
       end;

       case Shp.Pen.Style of
         psDash: Write('[ 10 4 ] 0 d');
         psDashDot: Write('[ 8 3 2 3 ] 0 d');
         psDashDotDot: Write('[ 7 3 2 2 2 3 ] 0 d');
         psDot: Write('[ 2 2 ] 0 d');
         psClear: Write(ColorToPDF(clWhite) + ' RG');
       end;

       if Shp.Brush.Color <> clBlack then begin
          Write(ColorToPDF(Shp.Brush.Color) + ' rg');
       end;

       if Shp.ShapeType in [stCircle, stEllipse] then begin
          XRadius := Width / 2;
          YRadius := Height / 2;
          Write(FmtFloat(Left + XRadius) + ' ' + FmtFloat(Top) + ' m');
          Write(ArcTo(Left + XRadius, Top, Left + Width, Top - YRadius, XRadius, YRadius));
          Write(ArcTo(Left + Width, Top - YRadius, Left + XRadius, Top - Height, XRadius, YRadius));
          Write(ArcTo(Left + XRadius, Top - Height, Left, Top - YRadius, XRadius, YRadius));
          Write(ArcTo(Left, Top - YRadius, Left + XRadius, Top, XRadius, YRadius));
       end;

       if Shp.ShapeType in [stSquare] then begin
          if Width > Height then begin
             Left := Left + (Width - Height) div 2;
             Write(IntToStr(Left) + ' ' + IntToStr(Top - Height) + ' ' + IntToStr(Width) + ' ' + IntToStr(Height) + ' re');
          end else begin
             Top := Top + (Height - Width) div 2;
             Write(IntToStr(Left) + ' ' + IntToStr(Top - Height) + ' ' + IntToStr(Width) + ' ' + IntToStr(Height) + ' re');
          end;
       end;

       if Shp.ShapeType in [stRoundSquare] then begin
          if Width > Height then begin
             Left := Left + (Width - Height) div 2;
          end else begin
             Top := Top + (Height - Width) div 2;
          end;
          RoundRect(Left, Top, Width, Height, ThousandthsToPoints(Shp.XCornerRound) div 2, ThousandthsToPoints(Shp.YCornerRound) div 2);
       end;

       if Shp.ShapeType in [stRoundRect] then begin
          RoundRect(Left, Top, Width, Height, ThousandthsToPoints(Shp.XCornerRound) div 2, ThousandthsToPoints(Shp.YCornerRound) div 2);
       end;

       if Shp.ShapeType in [stRectangle] then begin
          Write(IntToStr(Left) + '.5 ' + IntToStr(Top - Height) + ' ' + IntToStr(Width) + ' ' + IntToStr(Height) + ' re');
       end;

       if Shp.Pen.Style = psSolid then begin
          if Shp.Brush.Style = bsSolid then begin
             Write('B');
          end else begin
             Write('S');
          end;
       end else begin
          if Shp.Brush.Style = bsSolid then begin
             Write('f');
          end else begin
             Write('n');
          end;
       end;

       Write('Q');

    end;

    // Process lines

    if Itm.ItemType = riLine then begin

       ELeft   := EThousandthsToPoints(Itm.Left);
       EWidth  := EThousandthsToPoints(Itm.Width);
       EHeight := EThousandthsToPoints(Itm.Height);
       ETop    := PageHeight - EThousandthsToPoints(Itm.Top) - TopMargin;

       Write('q');

       Lne := TppDrawLine(Itm.DrawCmd);

       if Lne.LineStyle = lsSingle then begin
          N := 1;
       end else begin
          N := 2;
       end;

       XOffset := ELeft;
       YOffset := ETop;
       Weight  := Trunc(PixelsToPoints(Lne.Weight));

       Write(IntToStr(Weight) + ' w');

       case Lne.Pen.Style of
         psDash: Write('[ 10 4 ] 0 d');
         psDashDot: Write('[ 8 3 2 3 ] 0 d');
         psDashDotDot: Write('[ 7 3 2 2 2 3 ] 0 d');
         psDot: Write('[ 2 2 ] 0 d');
         psClear: Write(ColorToPDF(clWhite) + ' RG');
       end;

       if Lne.Pen.Color <> clBlack then begin
          Write(ColorToPDF(Lne.Pen.Color) + ' RG');
       end;

       for L := 1 to N do begin

         if Lne.LinePosition = lpTop then begin
            Write(FmtFloat(XOffset) + ' ' + FmtFloat(YOffset) + ' m');
            Write(FmtFloat(XOffset + EWidth) + ' ' + FmtFloat(YOffset) + ' l');
            YOffset := YOffset - (Weight + 3);
         end;

         if Lne.LinePosition = lpBottom then begin
            Write(FmtFloat(XOffset) + ' ' + FmtFloat(YOffset - 0.75) + ' m');
            Write(FmtFloat(XOffset + EWidth) + ' ' + FmtFloat(YOffset - 0.75) + ' l');
            YOffset := YOffset + (Weight + 3);
         end;

         if Lne.LinePosition = lpLeft then begin
            Write(FmtFloat(XOffset) + ' ' + FmtFloat(YOffset) + ' m');
            Write(FmtFloat(XOffset) + ' ' + FmtFloat(YOffset - EHeight) + ' l');
            XOffset := XOffset + (Weight + 3);
         end;

         if Lne.LinePosition = lpRight then begin
            Write(FmtFloat(XOffset + EWidth) + ' ' + FmtFloat(YOffset) + ' m');
            Write(FmtFloat(XOffset + EWidth) + ' ' + FmtFloat(YOffset - EHeight) + ' l');
            XOffset := XOffset - (Weight + 3);
         end;

         Write('S');

       end;

       Write('Q');

    end;

    //
    // Process RichText (as plain text for now)
    //

    if Itm.ItemType = riRTF then begin

       Font := TFont.Create;
       Font.Name := 'Arial';
       Font.Size := 9;

       Left   := ThousandthsToPoints(Itm.AdjLeft);
       Top    := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin - 12;

       Buffer := RTFToPlainString(TppDrawRichText(Itm.DrawCmd));

       Buffer := Replace(Buffer, '\', '\\');
       Buffer := Replace(Buffer, '(', '\(');
       Buffer := Replace(Buffer, ')', '\)');
       Buffer := Replace(Buffer, #13, ') Tj T* (');
       Buffer := Replace(Buffer, #10, '');

       Write('BT');

       Write('/F' + IntToStr(FontIndex(Font)) + ' ' + IntToStr(Font.Size) + ' Tf');
       Write(IntToStr(TextHeight(Font)) + ' TL');
       Write(IntToStr(Left) + ' ' + IntToStr(Top + 2) + ' Td (' + Buffer + ') Tj ');

       Write('ET');

       Font.Free;

    end;

    //
    // Process Text
    //

    if Itm.ItemType = riText then begin

       Left   := ThousandthsToPoints(Itm.AdjLeft);
       Height := ThousandthsToPoints(Itm.Height);

       Txt := TppDrawText(Itm.DrawCmd);

//  Removed 11/1/2000 For crosstab support
//       if Txt.IsMemo or Txt.WordWrap then begin
//          Top := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin - (Txt.Font.Size + 2);
//       end else begin
//          Top := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin - Height;
//       end;

       Top := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin - (Txt.Font.Size + 2);

       if Txt.IsMemo or Txt.WordWrap then begin
          Buffer := Replace(Txt.WrappedText.Text, EOP, '');
          if Pos(#13, Buffer) <> 0 then begin
             Left := ThousandthsToPoints(Itm.Left);   // Use true left in case or wordwrapped labels
             if Txt.Font.Size < 0 then begin
                Txt.Font.Size := Txt.Font.Size + 2;
             end else begin
                Txt.Font.Size := Txt.Font.Size - 2;
             end;
          end;
       end else begin
          Buffer := Txt.Text;
       end;

       if Txt.Transparent = False then begin
          TrueTop := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin - Height;
          Write('q');
          Write(ColorToPDF(Txt.Color) + ' rg');
          Write(IntToStr(ThousandthsToPoints(Itm.Left)) + ' ' + IntToStr(TrueTop) + ' ' + IntToStr(Width + 2) + ' ' + IntToStr(Height) + ' re');
          Write('f');
          Write('Q');
          Write(ColorToPDF(clBlack) + ' rg');
       end;

       if Txt.AutoSize = False then begin
          TrueTop := PageHeight - ThousandthsToPoints(Itm.Top) - TopMargin;
          Write('q');
          Write(IntToStr(Left) + ' ' + IntToStr(TrueTop) + ' m');
          Write(IntToStr(Left + Width) + ' ' + IntToStr(TrueTop) + ' l');
          Write(IntToStr(Left + Width) + ' ' + IntToStr(TrueTop - Height - 5) + ' l');
          Write(IntToStr(Left) + ' ' + IntToStr(TrueTop - Height - 5) + ' l');
          Write(IntToStr(Left) + ' ' + IntToStr(TrueTop) + ' l');
          Write('W');
          Write('n');
       end;

       Buffer := Replace(Buffer, '\', '\\');
       Buffer := Replace(Buffer, '(', '\(');
       Buffer := Replace(Buffer, ')', '\)');

       Buffer := Replace(Buffer, #13, ') Tj T* (');
       Buffer := Replace(Buffer, #10, '');

       Write('BT');

       if Txt.Font.Color <> clBlack then begin
          Write(ColorToPDF(Txt.Font.Color) + ' rg');
       end;

       Write('/F' + IntToStr(FontIndex(Txt.Font)) + ' ' + IntToStr(Txt.Font.Size) + ' Tf');
       Write(IntToStr(TextHeight(Txt.Font) - 1) + ' TL');
       Write(IntToStr(Left) + ' ' + IntToStr(Top + 2) + ' Td (' + Buffer + ') Tj ');

       if Txt.Font.Color <> clBlack then begin
          Write(ColorToPDF(clBlack) + ' rg');
       end;

       Write('ET');

       if fsUnderline in Txt.Font.Style then begin
          Write('1 w');
          if Txt.Font.Color <> clBlack then begin
             Write(ColorToPDF(Txt.Font.Color) + ' RG');
          end;
          Write(IntToStr(Left) + ' ' + IntToStr(Top) + ' m');
          Write(IntToStr(Left + ThousandthsToPoints(Itm.AdjWidth) + 1) + ' ' + IntToStr(Top) + ' l');
          if Txt.Font.Color <> clBlack then begin
             Write(ColorToPDF(clBlack) + ' RG');
          end;
          Write('S');
       end;

       if Txt.AutoSize = False then begin
          Write('Q');
       end;

    end;

  end;

  ImgObj.Free;

end;

procedure TDirectPDFDevice.RoundRect(Left, Top, Width, Height, XRad,
  YRad: Integer);
begin
  // Top
  Write(IntToStr(Left + XRad) + ' ' + IntToStr(Top) + ' m');
  Write(IntToStr(Left + Width - XRad) + ' ' + IntToStr(Top) + ' l');
  Write(ArcTo(Left + Width - XRad, Top, Left + Width, Top - YRad, XRad, YRad));
  // Right
  Write(IntToStr(Left + Width) + ' ' + IntToStr(Top - Height + YRad) + ' l');
  Write(ArcTo(Left + Width, Top - Height + YRad, Left + Width - XRad, Top - Height, XRad, YRad));
  // Bottom
  Write(IntToStr(Left + XRad) + ' ' + IntToStr(Top - Height) + ' l');
  Write(ArcTo(Left + XRad, Top - Height, Left, Top - Height + YRad, XRad, YRad));
  // Left
  Write(IntToStr(Left) + ' ' + IntToStr(Top - YRad) + ' l');
  Write(ArcTo(Left, Top - YRad, Left + XRad, Top, XRad, YRad));
end;

procedure TDirectPDFDevice.RunLength(Source, Target: TStream);
var
  Buffer, C, LastOut, LastBuf: String;
  LastCnt: Integer;
begin

  C := ' ';
  Buffer := '';
  LastOut := '';
  LastCnt := 0;
  Source.Position := 0;
  Target.Position := 0;

  while Source.Position < Source.Size do begin

    Source.Read(C[1], 1);

    if (C = LastOut) and (LastCnt <= 127) then begin
       if Length(LastBuf) > 0 then begin
          Buffer := Buffer + CHR(Length(LastBuf) - 1) + LastBuf;
          LastBuf := '';
       end;
       Inc(LastCnt);
    end else begin
       if LastCnt = 0 then begin
       end else if LastCnt > 1 then begin
          Buffer := Buffer + CHR(257 - LastCnt) + LastOut;
       end else begin
          LastBuf := LastBuf + LastOut;
          if Length(LastBuf) >= 128 then begin
             Buffer := Buffer + CHR(Length(LastBuf) - 1) + LastBuf;
             LastBuf := '';
          end;
       end;
       LastCnt := 1;
       LastOut := C;
    end;

    if Length(Buffer) > 0 then begin
       Target.Write(Buffer[1], Length(Buffer));
    end;
    Buffer := '';

  end;

  if Length(LastBuf) > 0 then begin
     Buffer := Buffer + CHR(Length(LastBuf) - 1) + LastBuf;
  end;

  if LastCnt = 1 then begin
     Buffer := Buffer + CHR(0) + LastOut;
  end;

  if LastCnt > 1 then begin
     Buffer := Buffer + CHR(257 - LastCnt) + LastOut;
  end;

  Buffer := Buffer + CHR(128) + '>';
  Target.Write(Buffer[1], Length(Buffer));

  Source.Position := 0;
  Target.Position := 0;
end;

procedure TDirectPDFDevice.StartBand;
begin
  inherited;
end;

procedure TDirectPDFDevice.StartJob;
begin
//  inherited;
  SeparateBands := False;

  ObjNo    := 4;
  MaxKids  := 10;
  FilePos  := 0;
  TotPages := 0;

  Write('%PDF-1.2');

  AddRef('1');

  Write('1 0 obj');
  Write('<< /Type /Catalog ');
  Write('/Pages 3 0 R >> ');
  Write('endobj');
end;

procedure TDirectPDFDevice.StartPage;
var
  PageObj: String;
begin
  inherited;

  PageHeight := ThousandthsToPoints(Page.PageDef.mmHeight);
  PageWidth  := ThousandthsToPoints(Page.PageDef.mmWidth);

  if TotPages mod MaxKids = 0 then begin
     PageList.Add(Copy(NextObj() + '      ', 1, 6) + ' ');
  end;
  TotPages := TotPages + 1;

  PageObj  := NextObj();
  PageList[PageList.Count - 1] := PageList[PageList.Count - 1] + PageObj + ' 0 R ';

  AddRef(PageObj);
  Write(PageObj + ' 0 obj');
  Write('<< /Type /Page ');
  Write('/Parent ' + Trim(Copy(PageList[PageList.Count - 1], 1, 6)) + ' 0 R ');
end;

procedure TDirectPDFDevice.StartStream;
begin
  InStream := True;
  InImage := False;
  StreamSize := 0;
end;

procedure TDirectPDFDevice.Write(Buffer: String);
begin
  if InStream then begin
     if InImage = False then begin
        Buffer := Buffer + #13 + #10;
     end;
     if Length(Buffer) > 0 then begin
        MemStream.Write(Buffer[1], Length(Buffer));
     end;
  end else begin
     inherited Write(Buffer + #13 + #10);
     StreamSize := StreamSize + Length(Buffer) + 2;
     FilePos := FilePos + Length(Buffer) + 2;
  end;
end;

procedure TDirectPDFDevice.WriteBMP(ImgObj: String; B: TBitmap;
  Mask: Boolean);
var
  Buffer, LenObj, StreamLen: String;
  I, J, K, X, Y: Integer;
  Line: PByteArray;
begin
  LenObj := NextObj();
  AddRef(ImgObj);

  Write(ImgObj + ' 0 obj');
  Write('<< /Type /XObject /Subtype /Image');
  Write('/Name /Im' + ImgObj);
  Write('/Width ' + IntToStr(B.Width) + ' /Height ' + IntToStr(B.Height));
  if Mask then begin
     Write('/ImageMask true');
     Write('/BitsPerComponent 1');
     Write('/Decode [0 1]');
     Write('/Interpolate true');
  end else begin
     Write('/BitsPerComponent 8');
     Write('/ColorSpace /DeviceRGB');
  end;

  {$IFDEF VER130}
  Write('/Filter [ /ASCII85Decode /FlateDecode ]');
  {$ELSE}
  Write('/Filter [ /ASCII85Decode /RunLengthDecode ]');
  {$ENDIF}

  Write('/Length ' + LenObj + ' 0 R >> ');
  Write('stream');
  StartStream;
  InImage := True;

  X := B.Height;

  if Mask then begin
     Y := Ceil(B.Width / 8);
  end else begin
     Y := B.Width;
  end;

  for I := 0 to X - 1 do begin

    Line := PByteArray(B.ScanLine[I]);

    for J := 0 to Y - 1 do begin
      if Mask then begin
         Buffer := Buffer + CHR(Line[J]);
      end else begin
         K := J * 3;
         Buffer := Buffer + CHR(Line[K + 2]);
         Buffer := Buffer + CHR(Line[K + 1]);
         Buffer := Buffer + CHR(Line[K + 0]);
      end;

      Write(Buffer);
      Buffer := '';
    end;

  end;

  Write(Buffer);

  EndStream;
  StreamLen := IntToStr(StreamSize);
  Write('endstream');
  Write('endobj');

  AddRef(LenObj);
  Write(LenObj + ' 0 obj');
  Write(StreamLen);
  Write('endobj');
end;

procedure TDirectPDFDevice.WriteCrossRef;
var
  I: Integer;
begin
  XRefPos := FilePos;

  CrossRef.Sort;

  Write('xref');
  Write('0 ' + IntToStr(CrossRef.Count + 1));
  Write('0000000000 65535 f');
  for I := 0 to CrossRef.Count - 1 do begin
    Write(Copy(CrossRef[I], 8, 10) + ' 00000 n');
  end;
end;

procedure TDirectPDFDevice.WriteFontTbl;
var
  I: Integer;
begin
  for I := 0 to FontTbl.Count - 1 do begin
    AddRef(FontObj[I]);
    Write(FontObj[I] + ' 0 obj');
    Write('<< /Type /Font ');
    Write('/Subtype /Type1 ');
    Write('/Name /F' + IntToStr(I) + ' ');
    Write('/BaseFont /' + FontTbl[I] + ' ');
    if FontTbl[I] = 'ZapfDingbats' then begin
       Write('/Encoding /StandardEncoding >>');
    end else begin
       Write('/Encoding /WinAnsiEncoding >>');
    end;
    Write('endobj');
  end;
end;

initialization
  ppRegisterDevice(TDirectPDFDevice);
finalization
  ppUnRegisterDevice(TDirectPDFDevice);
end.
