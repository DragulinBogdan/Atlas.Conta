object Reports: TReports
  OldCreateOrder = False
  OnCreate = ReportsCreate
  Left = 211
  Top = 213
  Height = 373
  Width = 466
  object Report: TfrxReport
    DotMatrixReport = False
    EngineOptions.MaxMemSize = 10000000
    IniFile = '\Software\Fast Reports'
    PreviewOptions.Buttons = [pbPrint, pbLoad, pbSave, pbExport, pbZoom, pbFind, pbOutline, pbPageSetup, pbTools, pbEdit, pbNavigator]
    PreviewOptions.Zoom = 1.000000000000000000
    PrintOptions.Printer = 'Default'
    ReportOptions.CreateDate = 38273.468152488430000000
    ReportOptions.LastChange = 38273.468152488430000000
    ScriptLanguage = 'PascalScript'
    ScriptText.Strings = (
      'begin'
      ''
      'end.')
    Left = 32
    Top = 16
    Datasets = <>
    Variables = <>
    Style = <>
    object Page1: TfrxReportPage
      Orientation = poLandscape
      PaperWidth = 147.997333333333400000
      PaperHeight = 104.986666666666700000
      PaperSize = 70
      LeftMargin = 10.000000000000000000
      RightMargin = 10.000000000000000000
      TopMargin = 10.000000000000000000
      BottomMargin = 10.000000000000000000
    end
  end
  object Designer: TfrxDesigner
    Restrictions = []
    Left = 126
    Top = 16
  end
  object BarCode: TfrxBarCodeObject
    Left = 32
    Top = 96
  end
  object OLE: TfrxOLEObject
    Left = 86
    Top = 96
  end
  object Chart: TfrxChartObject
    Left = 139
    Top = 96
  end
  object RTF: TfrxRichObject
    Left = 193
    Top = 96
  end
  object Cross: TfrxCrossObject
    Left = 247
    Top = 96
  end
  object CheckBox: TfrxCheckBoxObject
    Left = 301
    Top = 96
  end
  object Gradient: TfrxGradientObject
    Left = 354
    Top = 96
  end
  object DotMatr: TfrxDotMatrixExport
    GraphicFrames = False
    SaveToFile = False
    UseIniSettings = True
    Left = 86
    Top = 152
  end
  object Dialog: TfrxDialogControls
    Left = 314
    Top = 16
  end
  object ZIP: TfrxGZipCompressor
    Left = 408
    Top = 16
  end
  object ADO: TfrxADOComponents
    DefaultDatabase = ADOConnection1
    Left = 220
    Top = 16
  end
  object DbRtti: TfsDBRTTI
    Left = 32
    Top = 216
  end
  object DbCtrlsRtti: TfsDBCtrlsRTTI
    Left = 95
    Top = 216
  end
  object ScriptScr: TfsScript
    SyntaxType = 'PascalScript'
    Left = 157
    Top = 216
  end
  object PascalScr: TfsPascal
    Left = 220
    Top = 216
  end
  object CPlusPlusScr: TfsCPP
    Left = 283
    Top = 216
  end
  object JavaScr: TfsJScript
    Left = 345
    Top = 216
  end
  object BasicScr: TfsBasic
    Left = 408
    Top = 216
  end
  object ClassesRtti: TfsClassesRTTI
    Left = 32
    Top = 280
  end
  object GraphicRtti: TfsGraphicsRTTI
    Left = 95
    Top = 280
  end
  object FormsRtti: TfsFormsRTTI
    Left = 157
    Top = 280
  end
  object ExtCtrlsRtti: TfsExtCtrlsRTTI
    Left = 220
    Top = 280
  end
  object DialogsRtti: TfsDialogsRTTI
    Left = 283
    Top = 280
  end
  object AdoRtti: TfsADORTTI
    Left = 345
    Top = 280
  end
  object ChartRtti: TfsChartRTTI
    Left = 408
    Top = 280
  end
  object TextExp: TfrxTXTExport
    ScaleWidth = 1.000000000000000000
    ScaleHeight = 1.000000000000000000
    Borders = True
    Pseudogrpahic = False
    PageBreaks = True
    OEMCodepage = False
    EmptyLines = True
    LeadSpaces = True
    PrintAfter = False
    PrinterDialog = True
    UseSavedProps = True
    ShowProgress = True
    Left = 32
    Top = 152
  end
  object HTMLExp: TfrxHTMLExport
    FixedWidth = True
    Left = 139
    Top = 152
  end
  object XLSExp: TfrxXLSExport
    ShowProgress = True
    Left = 193
    Top = 152
  end
  object XMLExp: TfrxXMLExport
    ShowProgress = True
    Left = 247
    Top = 152
  end
  object RTFExp: TfrxRTFExport
    ShowProgress = True
    Left = 301
    Top = 152
  end
  object JpgExp: TfrxJPEGExport
    Left = 354
    Top = 152
  end
  object PDFExp: TfrxPDFExport
    Left = 408
    Top = 152
  end
  object TiffExp: TfrxTIFFExport
    Left = 408
    Top = 96
  end
  object ADOConnection1: TADOConnection
    ConnectionString = 
      'Provider=SQLOLEDB.1;Password=qazwsx;Persist Security Info=True;U' +
      'ser ID=sa;Initial Catalog=SLOBOZIA_FIZ;Data Source=NTdx'
    LoginPrompt = False
    Provider = 'SQLOLEDB.1'
    Left = 176
    Top = 40
  end
end
