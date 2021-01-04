using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SheetRaw
    {
        public SheetProperties properties;
        public GridData[] data = { };
        public GridRange[] merges = { };
        public ConditionalFormatRule[] conditionalFormats;
        public FilterView[] filterViews = { };
        public ProtectedRange[] protectedRanges;
        public BasicFilter basicFilter;
        public EmbeddedChart[] charts;
        public BandedRange[] bandedRanges = { };
        public DeveloperMetadata[] developerMetadata;
        public DimensionGroup[] rowGroups = { };
        public DimensionGroup[] dimensionGroups = { };
    }
}
