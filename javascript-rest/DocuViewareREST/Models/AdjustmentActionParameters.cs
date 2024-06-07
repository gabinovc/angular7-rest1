namespace DocuViewareREST.Models
{
    public class AdjustmentActionParameters
    {
        public int[] Pages { get; set; }
        public RegionOfInterest RegionOfInterest { get; set; }
        public int ContrastValue { get; set; }
    }
}