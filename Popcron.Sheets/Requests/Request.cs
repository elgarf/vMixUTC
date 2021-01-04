using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class Request
    {
        //public object kind;
        public AddSheetRequest addSheet;

        public void Set(object kind)
        {
            if (kind.GetType() == typeof(UpdateSpreadsheetPropertiesRequest) ||
                kind.GetType() == typeof(UpdateSheetPropertiesRequest) ||
                kind.GetType() == typeof(UpdateDimensionPropertiesRequest) ||
                kind.GetType() == typeof(UpdateNamedRangeRequest) ||
                kind.GetType() == typeof(RepeatCellRequest) ||
                kind.GetType() == typeof(AddNamedRangeRequest) ||
                kind.GetType() == typeof(DeleteNamedRangeRequest) ||
                kind.GetType() == typeof(AddSheetRequest) ||
                kind.GetType() == typeof(DeleteSheetRequest) ||
                kind.GetType() == typeof(AutoFillRequest) ||
                kind.GetType() == typeof(CutPasteRequest) ||
                kind.GetType() == typeof(CopyPasteRequest) ||
                kind.GetType() == typeof(MergeCellsRequest) ||
                kind.GetType() == typeof(UnmergeCellsRequest) ||
                kind.GetType() == typeof(UpdateBordersRequest) ||
                kind.GetType() == typeof(UpdateCellsRequest) ||
                kind.GetType() == typeof(AddFilterViewRequest) ||
                kind.GetType() == typeof(AppendCellsRequest) ||
                kind.GetType() == typeof(ClearBasicFilterRequest) ||
                kind.GetType() == typeof(DeleteDimensionRequest) ||
                kind.GetType() == typeof(DeleteEmbeddedObjectRequest) ||
                kind.GetType() == typeof(DeleteFilterViewRequest) ||
                kind.GetType() == typeof(DuplicateFilterViewRequest) ||
                kind.GetType() == typeof(DuplicateSheetRequest) ||
                kind.GetType() == typeof(FindReplaceRequest) ||
                kind.GetType() == typeof(InsertDimensionRequest) ||
                kind.GetType() == typeof(InsertRangeRequest) ||
                kind.GetType() == typeof(MoveDimensionRequest) ||
                kind.GetType() == typeof(UpdateEmbeddedObjectPositionRequest) ||
                kind.GetType() == typeof(PasteDataRequest) ||
                kind.GetType() == typeof(TextToColumnsRequest) ||
                kind.GetType() == typeof(UpdateFilterViewRequest) ||
                kind.GetType() == typeof(DeleteRangeRequest) ||
                kind.GetType() == typeof(AppendDimensionRequest) ||
                kind.GetType() == typeof(AddConditionalFormatRuleRequest) ||
                kind.GetType() == typeof(UpdateConditionalFormatRuleRequest) ||
                kind.GetType() == typeof(DeleteConditionalFormatRuleRequest) ||
                kind.GetType() == typeof(SortRangeRequest) ||
                kind.GetType() == typeof(SetDataValidationRequest) ||
                kind.GetType() == typeof(SetBasicFilterRequest) ||
                kind.GetType() == typeof(AddProtectedRangeRequest) ||
                kind.GetType() == typeof(UpdateProtectedRangeRequest) ||
                kind.GetType() == typeof(DeleteProtectedRangeRequest) ||
                kind.GetType() == typeof(AutoResizeDimensionsRequest) ||
                kind.GetType() == typeof(AddChartRequest) ||
                kind.GetType() == typeof(UpdateChartSpecRequest) ||
                kind.GetType() == typeof(UpdateBandingRequest) ||
                kind.GetType() == typeof(AddBandingRequest) ||
                kind.GetType() == typeof(DeleteBandingRequest) ||
                kind.GetType() == typeof(CreateDeveloperMetadataRequest) ||
                kind.GetType() == typeof(UpdateDeveloperMetadataRequest) ||
                kind.GetType() == typeof(DeleteDeveloperMetadataRequest) ||
                kind.GetType() == typeof(RandomizeRangeRequest) ||
                kind.GetType() == typeof(AddDimensionGroupRequest) ||
                kind.GetType() == typeof(DeleteDimensionGroupRequest) ||
                kind.GetType() == typeof(UpdateDimensionGroupRequest))
            {
                //this.kind = kind;
            }
            else
            {
                throw new Exception("Unsupported data type " + kind.GetType());
            }
        }
    }
}