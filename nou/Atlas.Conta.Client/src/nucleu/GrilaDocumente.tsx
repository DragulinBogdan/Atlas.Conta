import type { ComponentProps, ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';

export const INDICIU_GRILA = 'Click selectează rândul, dublu-click îl deschide.';

// Grila de citire a unui tip de document (43c): remote, cheia `Id` a ListDto-ului,
// dublu-click → `${ruta}/{Id}`. Coloanele le scrie felia (43a).
export function GrilaDocumente(props: {
  sursa: ComponentProps<typeof DataGrid>['dataSource'];
  ruta: string;
  inaltime?: string;
  pagini?: number[];
  children: ReactNode;
}) {
  const { sursa, ruta, inaltime, pagini = [25, 50, 100], children } = props;
  const navigheaza = useNavigate();

  return (
    <DataGrid
      dataSource={sursa}
      remoteOperations
      showBorders
      columnAutoWidth
      focusedRowEnabled
      height={inaltime}
      onRowDblClick={(e) => navigheaza(`${ruta}/${(e.data as { Id: string }).Id}`)}
    >
      <Sorting mode="multiple" />
      <FilterRow visible />
      <HeaderFilter visible><Search enabled /></HeaderFilter>
      <Paging defaultPageSize={pagini[0]} />
      <Pager showInfo showPageSizeSelector allowedPageSizes={pagini} />
      {children}
    </DataGrid>
  );
}
