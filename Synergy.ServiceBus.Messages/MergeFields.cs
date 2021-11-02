using System;

namespace Synergy.ServiceBus.Messages
{
    public class MergeFields
    {
        public Guid[] InternalDelinquencyId { get; set; }

        [MergeField("Event #")]
        public string[] Event { get; set; }

        [MergeField("State")]
        public string[] State { get; set; }

        [MergeField("County/Jurisdiction")]
        public string[] County { get; set; }

        [MergeField("Event Type")]
        public string[] EventType { get; set; }

        [MergeField("Auction Type")]
        public string[] AuctionType { get; set; }

        [MergeField("Funding Date")]
        public DateTime?[] FundingDate { get; set; }

        [MergeField("Sale Date")]
        public DateTime?[] SaleDate { get; set; }

        [MergeField("Sale Date Status")]
        public string[] SaleDateStatus { get; set; }

        [MergeField("Registration Deadline")]
        public DateTime?[] RegistrationDeadline { get; set; }

        [MergeField("Deposit Deadline")]
        public DateTime?[] DepositDeadline { get; set; }

        [MergeField("Deposit Amount")]
        public decimal[] DepositAmount { get; set; }

        [MergeField("Final Payment Type")]
        public string[] FinalPaymentType { get; set; }

        [MergeField("Treasurer Fee")]
        public decimal[] TreasurerFee { get; set; }

        [MergeField("Interest Rate", FormatString = "{0:0.00}%")]
        public decimal[] InterestRate { get; set; }

        [MergeField("Auction Address")]
        public string[] AuctionAddress { get; set; }

        [MergeField("Auction Start Time", FormatString = "{0:hh:mm tt}")]
        public DateTime?[] AuctionStartTime { get; set; }

        [MergeField("Treasurer Name")]
        public string[] TreasurerName { get; set; }

        [MergeField("Treasurer Email")]
        public string[] TreasurerEmail { get; set; }

        [MergeField("Estimated Purchase Amount")]
        public decimal[] EstimatedPurchaseAmount { get; set; }

        [MergeField("Estimated Deposit Amount")]
        public decimal[] EstimatedDepositAmount { get; set; }

        [MergeField("Refund Amount")]
        public decimal[] RefundAmount { get; set; }

        [MergeField("Delinquency Year")]
        public int[] DelinquencyYear { get; set; }

        [MergeField("Tax Ratio %", FormatString = "{0:p}")]
        public decimal[] TaxRatio { get; set; }

        [MergeField("Amount Due (Delinquency)")]
        public decimal[] AmountDue { get; set; }

        [MergeField("Amount Due (Property)")]
        public decimal[] PropertyAmountDue { get; set; }

        [MergeField("Amount Due (Record)")]
        public decimal[] LeadAmountDue { get; set; }

        [MergeField("Land Use Code")]
        public string[] LandUseCode { get; set; }

        [MergeField("General Land Use Code")]
        public string[] GeneralLandUseCode { get; set; }

        [MergeField("Internal Land Use Code")]
        public string[] InternalLandUseCode { get; set; }

        [MergeField("RU Amount")]
        public decimal[] RUAmount { get; set; }

        [MergeField("RU LTV%", FormatString = "{0:p}")]
        public decimal[] RULTV { get; set; }

        [MergeField("LTV%", FormatString = "{0:p}")]
        public decimal[] LTV { get; set; }

        [MergeField("Disp. Strategy")]
        public string[] DisplayStrategies { get; set; }

        [MergeField("Account Name (Owner)")]
        public string[] Owner { get; set; }

        [MergeField("Property Address")]
        public string[] PropertyAddress { get; set; }

        [MergeField("Property City")]
        public string[] PropertyCity { get; set; }

        [MergeField("Property State")]
        public string[] PropertyState { get; set; }

        [MergeField("Property Zip Code")]
        public string[] PropertyZipCode { get; set; }

        [MergeField("Parcel ID")]
        public string[] ParcelID { get; set; }

        [MergeField("Homestead")]
        public string[] Homestead { get; set; }

        [MergeField("Land Value")]
        public decimal[] LandValue { get; set; }

        [MergeField("Appraised Value (Property)")]
        public decimal[] AppraisedValue { get; set; }

        [MergeField("Appraised Value (Record)")]
        public decimal[] LeadAppraisedValue { get; set; }

        [MergeField("Building Value")]
        public decimal[] BuildingValue { get; set; }

        [MergeField("Land Acres")]
        public float[] LandAcres { get; set; }

        [MergeField("Building SqFt")]
        public int[] BuildingSqFt { get; set; }

        [MergeField("Year Built")]
        public int?[] YearBuilt { get; set; }

        [MergeField("Last Sale Date")]
        public DateTime?[] LastSaleDate { get; set; }

        [MergeField("Last Sale Amount")]
        public decimal[] LastSaleAmount { get; set; }

        [MergeField("Mortgage1 Loan")]
        public decimal[] Mortgage1Loan { get; set; }

        [MergeField("Mortgage1 Date")]
        public DateTime?[] Mortgage1Date { get; set; }

        [MergeField("Mortgage2 Loan")]
        public decimal[] Mortgage2Loan { get; set; }

        [MergeField("Mortgage2 Date")]
        public DateTime?[] Mortgage2Date { get; set; }

        [MergeField("Open Liens")]
        public int[] OpenLiens { get; set; }

        [MergeField("Closed Liens")]
        public int[] ClosedLiens { get; set; }

        [MergeField("Recent Buyer Name")]
        public string[] RecentBuyerName { get; set; }

        [MergeField("Recent Buyer Rate", FormatString = "{0:0.00}%")]
        public decimal[] RecentBuyerRate { get; set; }

        [MergeField("Legal Description")]
        public string[] LegalDescription { get; set; }

        [MergeField("Comment")]
        public string[] Comment { get; set; }

        [MergeField("Property Rating")]
        public decimal[] PropertyRating { get; set; }

        [MergeField("Area Rating")]
        public decimal[] AreaRating { get; set; }

        [MergeField("Occupied")]
        public bool?[] Occupied { get; set; }

        [MergeField("Roof Condition")]
        public string[] RoofCondition { get; set; }

        [MergeField("Lawn Maintained")]
        public string[] LawnMaintained { get; set; }

        [MergeField("Mailing Address 1")]
        public string[] MailingAddress1 { get; set; }

        [MergeField("Mailing Address 2")]
        public string[] MailingAddress2 { get; set; }

        [MergeField("Mailing Address 3")]
        public string[] MailingAddress3 { get; set; }

        [MergeField("Mailing City")]
        public string[] MailingCity { get; set; }

        [MergeField("Mailing State")]
        public string[] MailingState { get; set; }

        [MergeField("Mailing Zip Code")]
        public string[] MailingZipCode { get; set; }

        [MergeField("Bidder Number")]
        public string[] BidNumber { get; set; }

        [MergeField("Certification Number")]
        public string[] CertNo { get; set; }

        [MergeField("Tax Amount")]
        public decimal[] TaxAmount { get; set; }

        [MergeField("Overbid")]
        public decimal[] Overbid { get; set; }

        [MergeField("Premium")]
        public decimal[] Premium { get; set; }

        [MergeField("Recoverable Fees")]
        public decimal[] RecoverableFees { get; set; }

        [MergeField("Non-Recoverable Fees")]
        public decimal[] NonRecoverableFees { get; set; }

        [MergeField("Penalty Rate")]
        public decimal[] PenaltyRate { get; set; }

        [MergeField("Purchasing Entity")]
        public string[] PurchasingEntity { get; set; }

        [MergeField("Portfolio")]
        public string[] Portfolio { get; set; }

        [MergeField("Total Purchase Amount")]
        public decimal[] TotalPurchaseAmount { get; set; }

        [MergeField("Advertisement Batch")]
        public string[] AdvertisementBatch { get; set; }

        [MergeField("Advertisement Number")]
        public string[] AdvertisementNumber { get; set; }

        [MergeField("Campaign Name")]
        public string[] CampaignName { get; set; }

        [MergeField("Campaign Type")]
        public string[] CampaignType { get; set; }

        [MergeField("Campaign Sub Type")]
        public string[] CampaignSubType { get; set; }

        [MergeField("Campaign Created Date")]
        public string[] CreatedDate { get; set; }

        [MergeField("Campaign Description")]
        public string[] Description { get; set; }

        [MergeField("Campaign Target Date")]
        public string[] TargetDate { get; set; }

        [MergeField("Campaign Note")]
        public string[] Note { get; set; }

        [MergeField("Assigned User (Campaign)")]
        public string[] AssignedUser { get; set; }

        [MergeField("Do Not Contact")]
        public string[] DoNotContact { get; set; }

        /// <summary>
        ///  Gets or sets added Fields for Opportunity.
        /// </summary>
        [MergeField("Close Probability Percent", FormatString = "{0:0.00}%")]
        public decimal?[] CloseProbabilityPercent { get; set; }

        [MergeField("Origination Percent", FormatString = "{0:0.00}%")]
        public decimal?[] OriginationPercent { get; set; }

        [MergeField("Lender Credit")]
        public string[] LenderCredit { get; set; }

        [MergeField("Current Loan Balance")]
        public string[] CurrentLoanBalance { get; set; }

        [MergeField("ThirdParty Loan Balance")]
        public string[] ThirdPartyLoanBalance { get; set; }

        [MergeField("Term(In Months)")]
        public int?[] Term { get; set; }

        [MergeField("PrePay Months")]
        public int?[] PrePayMonths { get; set; }

        [MergeField("Prepay Percentage")]
        public decimal?[] PrepayPercentage { get; set; }

        [MergeField("Opportunity Stage")]
        public string[] OpportunityStage { get; set; }

        [MergeField("Opportunity Number")]
        public string[] OpportunityNumber { get; set; }

        [MergeField("Account Name(Record)")]
        public string[] AccountName { get; set; }

        [MergeField("Primary Contact Title")]
        public string[] PrimaryContactTitle { get; set; }

        [MergeField("Primary Contact First Name")]
        public string[] PrimaryContactFirstName { get; set; }

        [MergeField("Primary Contact Middle Name")]
        public string[] PrimaryContactMiddleName { get; set; }

        [MergeField("Primary Contact Last Name")]
        public string[] PrimaryContactLastName { get; set; }

        [MergeField("Primary Contact Cell Phone")]
        public string[] PrimaryContactCellPhone { get; set; }

        [MergeField("Primary Contact Email")]
        public string[] PrimaryContactEmail { get; set; }

        [MergeField("Primary Contact Address")]
        public string[] PrimaryContactAddress { get; set; }

        [MergeField("Primary Contact Type")]
        public string[] PrimaryContactType { get; set; }

        [MergeField("Primary Contact Office Phone")]
        public string[] PrimaryContactOfficePhone { get; set; }

        [MergeField("Opportunity Loan Type")]
        public string[] OpportunityLoanType { get; set; }

        [MergeField("Total Amount Due")]
        public string[] TotalAmountDue { get; set; }

        [MergeField("Opportunity Calculated Amount Due")]
        public string[] OpportunityCalculatedAmountDue { get; set; }

        [MergeField("Opportunity Adjusted Amount Due")]
        public string[] OpportunityAdjustedAmountDue { get; set; }

        [MergeField("Loan Officer")]
        public string[] LoanOfficer { get; set; }

        [MergeField("Properties")]
        public string[] Properties { get; set; }

        [MergeField("Opportunity Properties")]
        public string[] OpportunityProperties { get; set; }

        [MergeField("Opportunity Closed Date")]
        public string[] OpportunityClosedDate { get; set; }

        [MergeField("TaxID")]
        public string[] TaxId { get; set; }

        [MergeField("Borrowers 1 First Name")]
        public string[] Borrowers1FirstName { get; set; }

        [MergeField("Borrowers 1 Middle Name")]
        public string[] Borrowers1MiddleName { get; set; }

        [MergeField("Borrowers 1 Last Name")]
        public string[] Borrowers1LastName { get; set; }

        [MergeField("Borrowers 1 CellPhone")]
        public string[] Borrowers1CellPhone { get; set; }

        [MergeField("Borrowers 1 Work Phone")]
        public string[] Borrowers1WorkPhone { get; set; }

        [MergeField("Borrowers 1 Email")]
        public string[] Borrowers1Email { get; set; }

        [MergeField("Borrowers 1 Fax")]
        public string[] Borrowers1Fax { get; set; }

        [MergeField("Borrowers 1 Marital Status")]
        public string[] Borrowers1MaritalStatus { get; set; }

        [MergeField("Borrowers 1 Birth Date")]
        public string[] Borrowers1BirthDate { get; set; }

        [MergeField("Borrowers 1 Age")]
        public int?[] Borrowers1Age { get; set; }

        [MergeField("Borrowers 2 First Name")]
        public string[] Borrowers2FirstName { get; set; }

        [MergeField("Borrowers 2 Middle Name")]
        public string[] Borrowers2MiddleName { get; set; }

        [MergeField("Borrowers 2 Last Name")]
        public string[] Borrowers2LastName { get; set; }

        [MergeField("Borrowers 2 CellPhone")]
        public string[] Borrowers2CellPhone { get; set; }

        [MergeField("Borrowers 2 Work Phone")]
        public string[] Borrowers2WorkPhone { get; set; }

        [MergeField("Borrowers 2 Email")]
        public string[] Borrowers2Email { get; set; }

        [MergeField("Borrowers 2 Fax")]
        public string[] Borrowers2Fax { get; set; }

        [MergeField("Borrowers 2 Marital Status")]
        public string[] Borrowers2MaritalStatus { get; set; }

        [MergeField("Borrowers 2 Birth Date")]
        public string[] Borrowers2BirthDate { get; set; }

        [MergeField("Borrowers 2 Age")]
        public int?[] Borrowers2Age { get; set; }

        [MergeField("CommercialBorrowers")]
        public string[] CommercialBorrowers { get; set; }

        [MergeField("Opportunity Property Type")]
        public string[] OpportunityPropertyType { get; set; }

        [MergeField("Current Milestone")]
        public string[] CurrentMilestone { get; set; }

        [MergeField("Closing Cost")]
        public string[] ClosingCost { get; set; }

        [MergeField("Lead Source")]
        public string[] LeadSource { get; set; }

        [MergeField("File Date Started")]
        public string[] FileDateStarted { get; set; }

        [MergeField("Commercial Entity Borrower")]
        public string[] CommercialEntityBorrower { get; set; }

        [MergeField("Authorized Signor Title")]
        public string[] AuthorizedSignorTitle { get; set; }

        [MergeField("Authorized Signor First Name")]
        public string[] AuthorizedSignorFirstName { get; set; }

        [MergeField("Authorized Signor Middle Name")]
        public string[] AuthorizedSignorMiddleName { get; set; }

        [MergeField("Authorized Signor Last Name")]
        public string[] AuthorizedSignorLastName { get; set; }

        [MergeField("Authorized Signor CellPhone")]
        public string[] AuthorizedSignorCellPhone { get; set; }

        [MergeField("Authorized Signor Email")]
        public string[] AuthorizedSignorEmail { get; set; }

        [MergeField("Authorized Signor Fax")]
        public string[] AuthorizedSignorFax { get; set; }

        [MergeField("Commercial Entity Name")]
        public string[] CommercialEntityName { get; set; }

        [MergeField("Commercial Entity Tax ID")]
        public string[] CommercialEntityTaxId { get; set; }
    }
}
