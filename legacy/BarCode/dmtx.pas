unit dmtx;
interface
uses windows;
(*
translation of dmtx.h
libdmtx - Data Matrix Encoding/Decoding Library

Copyright (c) 2008 Mike Laughton

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

Contact: mike@dragonflylogic.com
translation by Jan Oosting:  j.oosting@lumc.nl
Jan 2008: start with encoding functionality
Mar 2008: working decoding and encoding
May 2008: translation to version 0.5.0
*)
// Version of dmtx.h that this file conforms to
//* $Id: dmtx.h 130 2008-04-15 18:17:22Z mblaughton $ */

// only the high level functions of the dll have been translated
// these are sufficient to create and scan datamatrix barcodes

const
DMTX_VERSION = '0.5.0';

DMTX_FAILURE = 0;
DMTX_SUCCESS = 1;

DMTX_STATUS_NOT_SCANNED =  0;
DMTX_STATUS_VALID       =  1;
DMTX_STATUS_INVALID     =  2;


DMTX_DISPLAY_SQUARE =   1;
DMTX_DISPLAY_POINT  =   2;
DMTX_DISPLAY_CIRCLE =   3;

DMTX_REGION_EOF      =   -1;
DMTX_REGION_NOT_FOUND=    0;
DMTX_REGION_FOUND    =    1;

DMTX_MODULE_OFF      =  $00;
DMTX_MODULE_ON_RED   =  $01;
DMTX_MODULE_ON_GREEN =  $02;
DMTX_MODULE_ON_BLUE  =  $04;
DMTX_MODULE_ON_RGB   =  $07;
DMTX_MODULE_UNSURE   =  $08;
DMTX_MODULE_ASSIGNED =  $10;
DMTX_MODULE_VISITED  =  $20;
DMTX_MODULE_DATA     =  $40;

DMTX_SYMBOL_SQUARE_AUTO = -1;
DMTX_SYMBOL_SQUARE_COUNT= 24;
DMTX_SYMBOL_RECT_AUTO   = -2;
DMTX_SYMBOL_RECT_COUNT  =  6;

type
(*typedef enum {
   DmtxDirNone       = 0x00,                        // = 0
   DmtxDirUp         = 0x01 << 0,                   //   1
   DmtxDirLeft       = 0x01 << 1,                   //   2
   DmtxDirDown       = 0x01 << 2,                   //   4
   DmtxDirRight      = 0x01 << 3,                   //   8
   DmtxDirHorizontal = DmtxDirLeft  | DmtxDirRight, //  10
   DmtxDirVertical   = DmtxDirUp    | DmtxDirDown,  //   5
   DmtxDirRightUp    = DmtxDirRight | DmtxDirUp,    //   9
   DmtxDirLeftDown   = DmtxDirLeft  | DmtxDirDown   //   6
} DmtxDirection;*)
DmtxDirection = (
  DmtxDirNone=0,
  DmtxDirUp=1,
  DmtxDirLeft=2,
  DmtxDirDown=4,
  DmtxDirVertical=5,
  DmtxDirLeftDown=6,
  DmtxDirRight=8,
  DmtxDirRightUp=9,
  DmtxDirHorizontal=10,
  DmtxDirLast=$10000);   // make sure sizeof is large enough in structures/records
                         // MingW = 4 bytes
                         // Delphi= 1 byte if high() < 256

DmtxEncodeMethod = (
  DmtxEncodeAutoBest,
  DmtxEncodeAutoFast,
  DmtxEncodeSingleScheme,
  DmtxEncodeLast=$10000);  // make sure sizeof is large enough

DmtxSchemeEncode =(
  DmtxSchemeEncodeAscii,
  DmtxSchemeEncodeC40,
  DmtxSchemeEncodeText,
  DmtxSchemeEncodeX12,
  DmtxSchemeEncodeEdifact,
  DmtxSchemeEncodeBase256,
  DmtxSchemeEncodeAutoBest,
  DmtxSchemeEncodeAutoFast,
  DmtxSchemeEncodeLast=$10000) ; // make sure sizeof is large enough

DmtxSchemeDecode = (
  DmtxSchemeDecodeAsciiStd,
  DmtxSchemeDecodeAsciiExt,
  DmtxSchemeDecodeC40,
  DmtxSchemeDecodeText,
  DmtxSchemeDecodeX12,
  DmtxSchemeDecodeEdifact,
  DmtxSchemeDecodeBase256);

//DmtxDecodeOptions =(
//  DmtxSingleScanOnly = 1,
//  DmtxDecodeOptionsLast=$10000); // make sure sizeof is large enough

DmtxSymAttribute =(
  DmtxSymAttribSymbolRows,
  DmtxSymAttribSymbolCols,
  DmtxSymAttribDataRegionRows,
  DmtxSymAttribDataRegionCols,
  DmtxSymAttribHorizDataRegions,
  DmtxSymAttribVertDataRegions,
  DmtxSymAttribMappingMatrixRows,
  DmtxSymAttribMappingMatrixCols,
  DmtxSymAttribDataWordLength,
  DmtxSymAttribErrorWordLength,
  DmtxSymAttribInterleavedBlocks);

DmtxCornerLoc=(
  DmtxCorner00 = 1,
  DmtxCorner10 = 2,
  DmtxCorner11 = 4,
  DmtxCorner01 = 8);

DmtxMatrix3 = array[0..2,0..2] of double;

DmtxMatrix3Struct_struct = record
  m :DmtxMatrix3;
end;

DmtxColor3 = record
  R :double;
  G :double;
  B :double;
end;

DmtxPixelLoc = record
   X :integer;
   Y :integer;
end;

PDmtxVector2=^DmtxVector2;
DmtxVector2 = record
  X :double;
  Y :double;
end ;

DmtxRay3 = record
  p :DmtxColor3;
  c :DmtxColor3;
end;

PDmtxRay2=^DmtxRay2;
DmtxRay2 = record
  isDefined  :char;
  tMin, tMax :double;
  p          :DmtxVector2 ;
  v          :DmtxVector2;
end;

DmtxGradient = record
  isDefined        :char;
  tMin, tMax, tMid :double;
  ray              :DmtxRay3;
  color, colorPrev :DmtxColor3 ; //* XXX maybe these aren't appropriate variables for a gradient? */
end;

DmtxPixel = record
  R :byte;
  G :byte;
  B :byte;
end;
pDmtxPixel=^DmtxPixel;

DmtxCompassDir = (
   DmtxCompassDirNeg45 = $01,
   DmtxCompassDir0     = $02,
   DmtxCompassDir45    = $04,
   DmtxCompassDir90    = $08,
   DmtxCompassLast     = $1000
) ;

DmtxCompassEdge = record
   assigned:char            ;
   magnitude:double          ;  //* sqrt(R^2 + G^2 + B^2) */
   intensity:DmtxColor3      ;
   edgeDir:DmtxCompassDir  ;
   scanDir:DmtxCompassDir  ;    //* DmtxCompassDir0 | DmtxCompassDir90 */
end;
PDmtxCompassEdge=^DmtxCompassEdge;

PDmtxImage=^DmtxImage;
PPDmtxImage=^PDmtxImage;
DmtxImage = record
  pageCount :cardinal;
  width     :cardinal;
  height    :cardinal;
  pxl       :pDmtxPixel;
  compass   :pDmtxCompassEdge;
end;

DmtxEdge= record
  offset :integer;
  t      :double;
  color  :DmtxColor3;
end;

DmtxChain= record
  tx, ty   :double;
  phi, shx :double ;
  scx, scy :double ;
  bx0, bx1 :double ;
  by0, by1 :double ;
  sz       :double ;
end;

DmtxCorners= record
  known :DmtxCornerLoc ; //* combination of (DmtxCorner00 | DmtxCorner10 | DmtxCorner11 | DmtxCorner01) */
  c00   :DmtxVector2 ;
  c10   :DmtxVector2 ;
  c11   :DmtxVector2 ;
  c01   :DmtxVector2 ;
end;

DmtxRegion = record
  found       :integer;
  gradient    :DmtxGradient; //* Linear blend of colors between background and symbol color */
  chain       :DmtxChain;    //* List of values that are used to build a transformation matrix */
  corners     :DmtxCorners;  //* Corners of barcode region */
  raw2fit     :DmtxMatrix3;  //* 3x3 transformation from raw image to fitted barcode grid */
  fit2raw     :DmtxMatrix3;  //* 3x3 transformation from fitted barcode grid to raw image */
  sizeIdx     :integer;      //* Index of arrays that store Data Matrix constants */
  symbolRows  :integer;      //* Number of total rows in symbol including alignment patterns */
  symbolCols  :integer;      //* Number of total columns in symbol including alignment patterns */
  mappingRows :integer;      //* Number of data rows in symbol */
  mappingCols :integer;      //* Number of data columns in symbol */
end;
PDmtxRegion=^DmtxRegion;

DmtxMessage = record
  arraySize   :integer;      //* mappingRows * mappingCols */
  codeSize    :integer;      //* Size of encoded data (data words + error words) */
  outputSize  :integer;      //* Size of buffer used to hold decoded data */
  outputIdx   :integer;      //* Internal index used to store output progress */
  _array      :pointer;      //unsigned char   *array;  /* Pointer to internal representation of scanned Data Matrix modules */
  code        :pointer;      //unsigned char   *code;   /* Pointer to internal storage of code words (data and error) */
  output      :pchar;        //unsigned char   *output; /* Pointer to internal storage of decoded output */
end;
pDmtxMessage = ^DmtxMessage;
ppDmtxMessage= ^pDmtxMessage;

DmtxEdgeFollower = record
  slope            :integer;
  turnCount        :integer;
  paraOffset       :integer;
  perpOffset       :double;
  tMin, tMid, tMax :double;
  ray              :DmtxRay3;
  line0, line1     :DmtxRay2;
  dir              :DmtxDirection;
end;

DmtxScanGrid = record
   //* set once */
   minExtent:integer;  //* Smallest cross size used in scan */
   maxExtent:integer;  //* Size of bounding grid region (2^N - 1) */
   xOffset:integer;    //* Offset to obtain image X coordinate */
   yOffset:integer;    //* Offset to obtain image Y coordinate */
   //* reset for each level */
   total:integer;      //* Total number of crosses at this size */
   extent:integer;     //* Length/width of cross in pixels */
   jumpSize:integer;   //* Distance in pixels between cross centers */
   pixelTotal:integer; //* Total pixel count within an individual cross path */
   startPos:integer;   //* X and Y coordinate of first cross center in pattern */
   //* reset for each cross */
   pixelCount:integer; //* Progress (pixel count) within current cross pattern */
   xCenter:integer;    //* X center of current cross pattern */
   yCenter:integer;    //* Y center of current cross pattern */
end;

PDmtxDecode = ^DmtxDecode;
PPDmtxDecode = ^PDmtxDecode;
DmtxDecode = record
  image                 :PDmtxImage;
  grid                  :DmtxScanGrid ;
end;

PDmtxEncode = ^DmtxEncode;
PPDmtxEncode = ^PDmtxEncode;
DmtxEncode = record
  moduleSize :integer;
  marginSize :integer;
  method     :DmtxEncodeMethod;
  scheme     :DmtxSchemeEncode;
  message    :pDmtxMessage;
  image      :PDmtxImage;
  matrix     :DmtxRegion;
  xfrm       :DmtxMatrix3;   //* XXX still necessary? */
  rxfrm      :DmtxMatrix3;   //* XXX still necessary? */
end ;

(*
typedef struct DmtxChannel_struct {
   DmtxSchemeEncode  encScheme;          /* current encodation scheme */
   int               invalid;            /* channel status (invalid if non-zero) */
   unsigned char     *inputPtr;          /* pointer to current input character */
   unsigned char     *inputStop;         /* pointer to position after final input character */
   int               encodedLength;      /* encoded length (units of 2/3 bits) */
   int               currentLength;      /* current length (units of 2/3 bits) */
   int               schemeStart;        /* currentLength value before writing 1st encoded word in current scheme */
   unsigned char     encodedWords[1558]; /* array of encoded codewords */
} DmtxChannel;

/* Wrap in a struct for fast copies */
typedef struct DmtxChannelGroup_struct {
   DmtxChannel channel[6];
} DmtxChannelGroup;

typedef struct DmtxTriplet_struct {
   unsigned char value[3];
} DmtxTriplet;

typedef struct DmtxQuadruplet_struct {
   unsigned char value[4];
} DmtxQuadruplet;
*)




var
//extern DmtxEncode dmtxEncodeStructInit(void);
dmtxEncodeStructInit: function:DmtxEncode;cdecl;
//extern void dmtxEncodeStructDeInit(DmtxEncode *enc);
dmtxEncodeStructDeInit:procedure(enc: PDmtxEncode);cdecl;
// extern int dmtxEncodeDataMatrix(DmtxEncode *enc, int n, unsigned char *s,int sizeIdxRequest);
dmtxEncodeDataMatrix:function(enc:PDmtxEncode;n:integer;s:pchar;sizeIdxRequest:integer):integer;cdecl;
//extern int dmtxEncodeDataMosaic(DmtxEncode *enc, int n, unsigned char *s, int sizeIdxRequest);}
dmtxEncodeDataMosaic:function(enc:PDmtxEncode;n:integer;s:pchar;sizeIdxRequest:integer):integer;cdecl;

//* dmtxdecode.c */
//extern DmtxDecode dmtxDecodeStructInit(DmtxImage *img, DmtxPixelLoc p0, DmtxPixelLoc p1, int gap);
dmtxDecodeStructInit:function(img:pDmtxImage;p0: DmtxPixelLoc;p1: DmtxPixelLoc; gap:integer):DmtxDecode;cdecl;
//extern void dmtxDecodeStructDeInit(DmtxDecode *dec);
dmtxDecodeStructDeInit:procedure(dec:pDmtxDecode);cdecl;
//extern DmtxMessage *dmtxDecodeMatrixRegion(DmtxDecode *dec, DmtxRegion *reg, int fix);
dmtxDecodeMatrixRegion:function(dec:pDmtxDecode;reg: pDmtxRegion; fix:integer):pDmtxMessage;cdecl;
//extern DmtxMessage *dmtxMessageMalloc(int sizeIdx);
dmtxMessageMalloc:function(sizeIdx:integer):pDmtxMessage;cdecl;
//extern void dmtxMessageFree(DmtxMessage **mesg);
dmtxMessageFree:procedure(mesg:ppDmtxMessage);cdecl;

//* dmtxregion.c */
//extern DmtxRegion dmtxDecodeFindNextRegion(DmtxDecode *decode);
dmtxDecodeFindNextRegion:function(decode:PDmtxDecode):DmtxRegion;cdecl;
//extern DmtxRegion dmtxScanPixel(DmtxDecode *decode, DmtxPixelLoc loc);
dmtxScanPixel:function(decode:PDmtxDecode;loc:DmtxPixelLoc):DmtxRegion;cdecl;

//* dmtximage.c */
//extern DmtxImage *dmtxImageMalloc(int width, int height);
dmtxImageMalloc:function(width,height:integer):pDmtxImage;cdecl;
//extern int dmtxImageFree(DmtxImage **img);
dmtxImageFree:function(img:PPDmtxImage):integer;cdecl;
//extern int dmtxImageGetWidth(DmtxImage *img);
dmtxImageGetWidth:function(img:PDmtxImage):integer;cdecl;
//extern int dmtxImageGetHeight(DmtxImage *img);
dmtxImageGetHeight:function(img:PDmtxImage):integer;cdecl;
//extern int dmtxImageGetOffset(DmtxImage *img, DmtxDirection dir, int lineNbr, int offset);
dmtxImageGetOffset:function(img:PDmtxImage;dir:DmtxDirection ;lineNbr:integer;offset:integer):integer;cdecl;

var
dmtxVector2AddTo: function(v1, v2 :PDmtxVector2):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2AddTo(DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2Add:   function(vOut, v1, v2 :PDmtxVector2):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2Add(DmtxVector2 *vOut, DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2SubFrom: function(v1, v2 :PDmtxVector2):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2SubFrom(DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2Sub:   function(vOut, v1, v2 :PDmtxVector2):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2Sub(DmtxVector2 *vOut, DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2ScaleBy: function(v :PDmtxVector2; s :double):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2ScaleBy(DmtxVector2 *v, double s);
dmtxVector2Scale: function(vOut, v :PDmtxVector2; s :double):PDmtxVector2;cdecl;
//extern DmtxVector2 *dmtxVector2Scale(DmtxVector2 *vOut, DmtxVector2 *v, double s);
dmtxVector2Cross: function(v1, v2 :PDmtxVector2):double;cdecl;
//extern double dmtxVector2Cross(DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2Norm: function(v :PDmtxVector2):double;cdecl;
//extern double dmtxVector2Norm(DmtxVector2 *v);
dmtxVector2Dot: function(v1, v2 :PDmtxVector2):double;cdecl;
//extern double dmtxVector2Dot(DmtxVector2 *v1, DmtxVector2 *v2);
dmtxVector2Mag: function(v :PDmtxVector2):double;cdecl;
//extern double dmtxVector2Mag(DmtxVector2 *v);
dmtxDistanceFromRay2: function(r :PDmtxRay2; q:PDmtxVector2):double;cdecl;
//extern double dmtxDistanceFromRay2(DmtxRay2 *r, DmtxVector2 *q);
dmtxDistanceAlongRay2: function(r :PDmtxRay2; q:PDmtxVector2):double;cdecl;
//extern double dmtxDistanceAlongRay2(DmtxRay2 *r, DmtxVector2 *q);
dmtxRay2Intersect: function(point:PDmtxVector2; p0,p1 :PDmtxRay2):integer;cdecl;
//extern int dmtxRay2Intersect(DmtxVector2 *point, DmtxRay2 *p0, DmtxRay2 *p1);
dmtxPointAlongRay2: function(point:PDmtxVector2; r :PDmtxRay2; t :double):integer;cdecl;
//extern int dmtxPointAlongRay2(DmtxVector2 *point, DmtxRay2 *r, double t);

(*extern void dmtxMatrix3Copy(DmtxMatrix3 m0, DmtxMatrix3 m1);
extern void dmtxMatrix3Identity(DmtxMatrix3 m);
extern void dmtxMatrix3Translate(DmtxMatrix3 m, double tx, double ty);
extern void dmtxMatrix3Rotate(DmtxMatrix3 m, double angle);
extern void dmtxMatrix3Scale(DmtxMatrix3 m, double sx, double sy);
extern void dmtxMatrix3Shear(DmtxMatrix3 m, double shx, double shy);
extern DmtxVector2 *dmtxMatrix3VMultiplyBy(DmtxVector2 *v, DmtxMatrix3 m);
extern DmtxVector2 *dmtxMatrix3VMultiply(DmtxVector2 *vOut, DmtxVector2 *vIn, DmtxMatrix3 m);
extern void dmtxMatrix3Multiply(DmtxMatrix3 mOut, DmtxMatrix3 m0, DmtxMatrix3 m1);
extern void dmtxMatrix3MultiplyBy(DmtxMatrix3 m0, DmtxMatrix3 m1);
extern void dmtxMatrix3LineSkewTop(DmtxMatrix3 m, double b0, double b1, double sz);
extern void dmtxMatrix3LineSkewTopInv(DmtxMatrix3 m, double b0, double b1, double sz);
extern void dmtxMatrix3LineSkewSide(DmtxMatrix3 m, double b0, double b1, double sz);
extern void dmtxMatrix3LineSkewSideInv(DmtxMatrix3 m, double b0, double b1, double sz);
extern void dmtxMatrix3Print(DmtxMatrix3 m);

extern DmtxPixel dmtxPixelFromImage(DmtxImage *image, int x, int y);
extern void dmtxColor3FromImage2(DmtxColor3 *color, DmtxImage *image, DmtxVector2 p);
extern DmtxColor3 *dmtxColor3FromPixel(DmtxColor3 *color, DmtxPixel *pxl);
extern void dmtxPixelFromColor3(DmtxPixel *pxl, DmtxColor3 *color);
extern DmtxColor3 dmtxColor3AlongRay3(DmtxRay3 *ray, double dist);
extern DmtxColor3 *dmtxColor3AddTo(DmtxColor3 *v1, DmtxColor3 *v2);
extern DmtxColor3 *dmtxColor3Add(DmtxColor3 *vOut, DmtxColor3 *v1, DmtxColor3 *v2);
extern DmtxColor3 *dmtxColor3SubFrom(DmtxColor3 *v1, DmtxColor3 *v2);
extern DmtxColor3 *dmtxColor3Sub(DmtxColor3 *vOut, DmtxColor3 *v1, DmtxColor3 *v2);
extern DmtxColor3 *dmtxColor3ScaleBy(DmtxColor3 *v, double s);
extern DmtxColor3 *dmtxColor3Scale(DmtxColor3 *vOut, DmtxColor3 *v, double s);
extern DmtxColor3 *dmtxColor3Cross(DmtxColor3 *vOut, DmtxColor3 *v1, DmtxColor3 *v2);
extern double dmtxColor3Norm(DmtxColor3 *v);
extern double dmtxColor3Dot(DmtxColor3 *v1, DmtxColor3 *v2);
extern double dmtxColor3Mag(DmtxColor3 *v);
extern double dmtxDistanceFromRay3(DmtxRay3 *r, DmtxColor3 *q);
extern double dmtxDistanceAlongRay3(DmtxRay3 *r, DmtxColor3 *q);
extern int dmtxPointAlongRay3(DmtxColor3 *point, DmtxRay3 *r, double t);
*)
var
//extern int dmtxSymbolModuleStatus(DmtxMessage *mapping, int sizeIdx, int row, int col);
dmtxSymbolModuleStatus:function(mapping:pDmtxMessage;sizeIdx,row,col:integer):integer;cdecl;
//extern int dmtxGetSymbolAttribute(int attribute, int sizeIdx);
dmtxGetSymbolAttribute:function(attribute:DmtxSymAttribute;sizeIdx:integer):integer;cdecl;
// extern char *dmtxVersion(void);
dmtxVersion:function:pchar;




var
  dmtxDLLLoaded: Boolean = False;

implementation
var
  SaveExit: pointer;
  DLLHandle: THandle;
  ErrorMode: Integer;

  procedure NewExit; far;
  begin
    ExitProc := SaveExit;
    FreeLibrary(DLLHandle)
  end {NewExit};

function dmtxImageDeInit(var image: DmtxImage):integer;
begin
  if assigned(image.pxl) then
    FreeMem(image.pxl);
  image.pxl:=nil;
  image.width:=0;
  image.pageCount:=0;
  image.height:=0;
  result:=0;
end;

procedure LoadDmtxDLL;
begin
  if dmtxDLLLoaded then Exit;
  ErrorMode := SetErrorMode($8000{SEM_NoOpenFileErrorBox});
  DLLHandle := LoadLibrary('dmtx.dll');
  if DLLHandle >= 32 then
  begin
    dmtxDLLLoaded := True;
    SaveExit := ExitProc;
    ExitProc := @NewExit;
(* S+R template for method entrypoints
    @dmtx := GetProcAddress(DLLHandle,'dmtx');
    Assert(@dmtx <> nil);

*)
    // dmtxencode
    @dmtxEncodeStructInit := GetProcAddress(DLLHandle,'dmtxEncodeStructInit');
    Assert(@dmtxEncodeStructInit <> nil);

    @dmtxEncodeStructDeInit := GetProcAddress(DLLHandle,'dmtxEncodeStructDeInit');
    Assert(@dmtxEncodeStructDeInit <> nil);

    @dmtxEncodeDataMatrix := GetProcAddress(DLLHandle,'dmtxEncodeDataMatrix');
    Assert(@dmtxEncodeDataMatrix <> nil);

    @dmtxEncodeDataMosaic := GetProcAddress(DLLHandle,'dmtxEncodeDataMosaic');
    Assert(@dmtxEncodeDataMosaic <> nil);

    //dmtxDecode
    @dmtxDecodeStructInit := GetProcAddress(DLLHandle,'dmtxDecodeStructInit');
    Assert(@dmtxDecodeStructInit <> nil);

    @dmtxDecodeStructDeInit := GetProcAddress(DLLHandle,'dmtxDecodeStructDeInit');
    Assert(@dmtxDecodeStructDeInit <> nil);

    @dmtxDecodeMatrixRegion := GetProcAddress(DLLHandle,'dmtxDecodeMatrixRegion');
    Assert(@dmtxDecodeMatrixRegion <> nil);

    @dmtxMessageMalloc := GetProcAddress(DLLHandle,'dmtxMessageMalloc');
    Assert(@dmtxMessageMalloc <> nil);

    @dmtxMessageFree := GetProcAddress(DLLHandle,'dmtxMessageFree');
    Assert(@dmtxMessageFree <> nil);

    // dmtxregion
    @dmtxDecodeFindNextRegion := GetProcAddress(DLLHandle,'dmtxDecodeFindNextRegion');
    Assert(@dmtxDecodeFindNextRegion <> nil);

    @dmtxScanPixel := GetProcAddress(DLLHandle,'dmtxScanPixel');
    Assert(@dmtxScanPixel <> nil);

    // dmtximage
    @dmtxImageMalloc := GetProcAddress(DLLHandle,'dmtxImageMalloc');
    Assert(@dmtxImageMalloc <> nil);

    @dmtxImageFree := GetProcAddress(DLLHandle,'dmtxImageFree');
    Assert(@dmtxImageFree <> nil);

    @dmtxImageGetWidth := GetProcAddress(DLLHandle,'dmtxImageGetWidth');
    Assert(@dmtxImageGetWidth <> nil);

    @dmtxImageGetHeight := GetProcAddress(DLLHandle,'dmtxImageGetHeight');
    Assert(@dmtxImageGetHeight <> nil);

    @dmtxImageGetOffset := GetProcAddress(DLLHandle,'dmtxImageGetOffset');
    Assert(@dmtxImageGetOffset <> nil);

    // misc
    @dmtxSymbolModuleStatus := GetProcAddress(DLLHandle,'dmtxSymbolModuleStatus');
    Assert(@dmtxSymbolModuleStatus <> nil);

    @dmtxGetSymbolAttribute := GetProcAddress(DLLHandle,'dmtxGetSymbolAttribute');
    Assert(@dmtxGetSymbolAttribute <> nil);
  end
  else
  begin
    dmtxDLLLoaded := False;
  end;
  SetErrorMode(ErrorMode)
end {LoadDLL};

begin
  LoadDmtxDLL;
end.
