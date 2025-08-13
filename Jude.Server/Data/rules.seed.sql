INSERT INTO
    "Rules" (
        "Id",
        "CreatedAt",
        "Name",
        "Description",
        "Status",
        "CreatedById"
    )
VALUES (
        'a1b2c3d4-e5f6-7890-1234-567890abcdef',
        now(),
        'Turnaround Time',
        'Adherence to claims turnaround time of four days by auto assessment of +/-95% of the claims. (Overview, page 4)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b2c3d4e5-f6a7-8901-2345-67890abcdef1',
        now(),
        'Claims Loss Ratio',
        'Claims loss ratio to remain within target (current target is 80%). (Overview, page 4)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c3d4e5f6-a7b8-9012-3456-7890abcdef12',
        now(),
        'Claim Validation Failure',
        'Where validation of claim header and line details fails, the claims should be rejected. EDI claims are validated at the point of switching into the system the claim should not go through if the details below are incorrect of missing. (section 1, page 5)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd4e5f6a7-b8c9-0123-4567-890abcdef123',
        now(),
        'Referring Provider for Specialist Services',
        'A referring service provider number is required for specialist services. (section 1.1, page 5)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e5f6a7b8-c9d0-1234-5678-90abcdef1234',
        now(),
        'Biometric or OTP for Online Claims',
        'Biometric signature check for EDI claims or One Time Pin check- required for online claims. NB Exemption to the rules apply to some service providers, members or disciplines. (section 1.1, page 5)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f6a7b8c9-d0e1-2345-6789-0abcdef12345',
        now(),
        'Tariff Code Specificity',
        'Tariffs are specific for claim types and service provider disciplines such that if an incorrect tariff is used, the claim line is auto rejected by the system, e.g., a specialist consultation tariff used on a general practitioner''s claim. (section 1.2, page 5)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a7b8c9d0-e1f2-3456-7890-bcdef1234567',
        now(),
        'Tariff/Nappi/CPT Code Validation',
        'The system should validate the tariff/nappi/CPT codes for accuracy, validity, and application of other tariff related rules e.g., age, gender etc. (section 1.2, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b8c9d0e1-f2a3-4567-8901-cdef12345678',
        now(),
        'ICD-10 Code Mandate and Exceptions',
        'ICD-10 codes are mandatory for all disciplines with the exception of the following disciplines: Government hospital- 202, Municipal Clinics- 204, Mission Hospitals- 207, Pharmacies- 060. (section 1.3, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c9d0e1f2-a3b4-5678-9012-def123456789',
        now(),
        'ICD-10 Code Rejection',
        'The system should auto reject claims where the ICD10 code is not specified. Claims with incorrect ICD10 codes should be rejected. (section 1.3, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd0e1f2a3-b4c5-6789-0123-ef1234567890',
        now(),
        'Treatment Date Validation',
        'The treatment date refers to the service date and the field is mandatory. The date cannot be future dated or be after the date the claim was received at Cimas. The field is also used to validate the stale period rule. (section 1.5, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e1f2a3b4-c5d6-7890-1234-f12345678901',
        now(),
        'Date & Time Sensitive Tariffs',
        'Private hospital outpatient tariffs are paid based on the day i.e., working day, after hours, weekends, public holidays etc. This rule applies to tariff range 02201 to 02259. A field is required to capture the time in and time out. (section 1.7, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f2a3b4c5-d6e7-8901-2345-123456789012',
        now(),
        'Modifier Application and Calculation',
        'Modifiers either increase or decrease the tariff award by a certain percentage. The system should auto calculate the modified amount accordingly. Certain modifiers require manual adjudication. (section 1.8, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a3b4c5d6-e7f8-9012-3456-234567890123',
        now(),
        'Multiple Procedures without Modifier',
        'Where more than one procedure is done on the same day by the same provider and no modifier is applied, the claim should be routed for manual assessment. (section 1.8, page 6)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b4c5d6e7-f8a9-0123-4567-345678901234',
        now(),
        'Stale Period - General (Zimbabwe)',
        'Claims for treatment rendered in Zimbabwe must be submitted to Cimas within 90 days from the date of treatment. This rule applies to all claim types and service provider disciplines. (section 2.1, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c5d6e7f8-a9b0-1234-5678-456789012345',
        now(),
        'Stale Period - Exceptions',
        'The stale period for government hospitals (disciplines 202 and 207) and for foreign claims (service provider numbers 91776, 91782 and 91774) is 120 days. (section 2.1, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd6e7f8a9-b0c1-2345-6789-567890123456',
        now(),
        'Stale Claim Rejection and Override',
        'The system should auto reject stale claims. However, functionality is required to override stale claims for cases approved by management. (section 2.1, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e7f8a9b0-c1d2-3456-7890-678901234567',
        now(),
        'Referring Provider Rule for Specialists',
        'All claims for specialist consultation and/or treatment must show evidence that the patient has been referred. The referring provider field should be mandatory for all specialist disciplines at capture level. (section 2.2, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f8a9b0c1-d2e3-4567-8901-789012345678',
        now(),
        'Self-Referral Prohibition',
        'Self-referral is not allowed i.e., a doctor cannot use his own practice number to refer patients to himself. The system should reject such claims. (section 2.2, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a9b0c1d2-e3f4-5678-9012-890123456789',
        now(),
        'Radiology Referral Restriction',
        'Radiology- A General Practitioner (discipline 014) is not allowed to refer for MRI or CT. (section 2.2, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b0c1d2e3-f4a5-6789-0123-901234567890',
        now(),
        'Orthodontics Referral Requirement',
        'Orthodontics- referral is required for orthodontic treatment. (section 2.2, page 7)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c1d2e3f4-a5b6-7890-1234-abcdef123456',
        now(),
        'Preauthorisation Flagging for Unauthorised Claims',
        'For a list of procedures that currently require preauthorisation, the system should flag the claims for review if there is no authorisation. (section 2.4, page 8)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd2e3f4a5-b6c7-8901-2345-bcdef1234567',
        now(),
        'High Value Claim Check',
        'The high value check is a control that is applied in the system whereby high value limits are set per claim for different disciplines. The current high value limits are: Pathology - US$200.00, Radiology- US$200.00, Other Disciplines US$2,000. (section 2.5, page 8)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e3f4a5b6-c7d8-9012-3456-cdef12345678',
        now(),
        'GP Dispensing Licence Check',
        'A General Practitioners (discipline 014) is allowed to dispense medicines if one has a valid drug dispensing licence. Where the service provider has no valid licence, the claim is rejected. (section 2.6, page 8)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f4a5b6c7-d8e9-0123-4567-def123456789',
        now(),
        'Maximum Quantity for Medication',
        'This rule applies to drug codes only. Claims that exceed that maximum number of drugs specified to be routed for manual adjudication with an option to override the rule where there is proof that the doctor has prescribed such quantities. (section 2.7, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a5b6c7d8-e9f0-1234-5678-ef1234567890',
        now(),
        'Early Refill Check for Repeat Prescription Medicines',
        'Members on chronic drugs obtain drugs from pharmacies monthly and are allowed to refill the prescription 25 days from the previous claim. Claims that are made within 25 days should be routed for manual adjudication, with an option to override the rule for approved cases. (section 2.8, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b6c7d8e9-f0a1-2345-6789-f12345678901',
        now(),
        'Drug-Drug Interaction (DDI) Checks',
        'This rule applies to medication that cannot be taken or prescribed at the same time. The system will flag such drugs and route them for manual adjudication. (section 2.9, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c7d8e9f0-a1b2-3456-7890-123456789012',
        now(),
        'Age-Based Check',
        'The tariffs are for procedures that can be done for the specified age groups. Where the dependent does not fall within the age group, the claims would be auto rejected with no option to override. (section 2.10, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd8e9f0a1-b2c3-4567-8901-234567890123',
        now(),
        'Gender Check',
        'The tariffs with a gender indicator are for procedures done on the specified gender. Where the dependent on the claim is of a different gender than the one specified the claim would be rejected with no option to override. (section 2.11, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e9f0a1b2-c3d4-5678-9012-345678901234',
        now(),
        'Tooth Number Check for Dental Claims',
        'The tooth number check is a rule to be applied to dental claims where the tariff code or procedure requires a tooth number. Where a tooth number is not indicated the claim line is auto rejected by the system. (section 2.12, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f0a1b2c3-d4e5-6789-0123-456789012345',
        now(),
        'Access Levels by Discipline',
        'Each product is designed to access services at specific service provider disciplines. Where one accesses service from a discipline that is not allowed, the system should reject the claim. However, there should be functionality to override the disciplines for approved exceptional cases. (section 2.13, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '0a1b2c3d-4e5f-7890-1234-567890123456',
        now(),
        'Biometric Signature Check for Online Claims',
        'There are claims that are submitted online and should be biometrically signed by the member. The system should auto reject the claim if the signature is not attached within 72 hours from date of submission. (section 2.14, page 9)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '1b2c3d4e-5f6a-8901-2345-678901234567',
        now(),
        'OTP Verification for Biometric Bypass',
        'One time pin member verification is used for members with fingerprints that cannot be read by the biometric readers e.g., the elderly, over 60 years, amputees etc. The system should allow online claims with OTP verification to be processed without biometric signatures. (section 2.15, page 9 and page 10)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '2c3d4e5f-6a7b-9012-3456-789012345678',
        now(),
        'Claim Attachment Check for Online Claims',
        'There are online claims for certain disciplines that require attachments. Where the attachments are mandatory the system should auto reject the claim if the attachment is not linked within 72 hours from date of submission. Certain service providers maybe exempted. (section 2.16, page 10)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '3d4e5f6a-7b8c-0123-4567-890123456789',
        now(),
        'Application of Claim Rejection Reasons',
        'Where a claim has been rejected either by the system or manually the system should be configured to apply the corresponding rejection reason/s. The rejection reason should be applied once the claim has gone been fully assessed. (section 2.17, page 10)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '4e5f6a7b-8c9d-1234-5678-901234567890',
        now(),
        'Independent Procedures (IND)',
        'If another procedure is done at the same time as the independent procedure but through same incision, the line is rejected. Tariff codes with an independent procedure adjudication indicator should be routed for manual review. (section 3.1, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '5f6a7b8c-9d0e-2345-6789-012345678901',
        now(),
        'By Report Procedures (BR)',
        'The BR adjudication indicator shows that the award for the services is to be determined by a doctor''s report because the service is too unusual to be assigned a fee. The tariffs are routed for manual assessment. (section 3.2, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '6a7b8c9d-0e1f-3456-7890-123456789012',
        now(),
        'Fee For Service (FFS)',
        'A tariff with an FFS indicator has no award assigned it. The award is calculated as sum of various services rendered. Manual adjudication is required. (section 3.3, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '7b8c9d0e-1f2a-4567-8901-234567890123',
        now(),
        'Follow Up Days for Surgical Procedures',
        'Some tariff codes for surgical procedures have a number of follow up days. The amount paid for that tariff is inclusive of the post operative care such that the same service provider who operated on the patient should not claim for review of the patient until the follow up days are over. (section 3.4, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '8c9d0e1f-2a3b-5678-9012-345678901234',
        now(),
        'Mutually Exclusive Tariffs (MCR)',
        'Mutually exclusive tariffs or multi-code rule (MCR) are generally tariff codes that cannot be claimed together on the same day by the same service provider. The subsequent claim should be rejected and routed for manual adjudication. (section 3.5, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '9d0e1f2a-3b4c-6789-0123-456789012345',
        now(),
        'Asterisked Procedures',
        'The tariff codes with an asterisk check are booked procedures that cannot be claimed on the same service date as tariff 90051 for the same service provider. When the tariffs are claimed on the same day as 90051 the claim line should automatically reject. (section 3.6, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd0e1f2a3-b4c5-d6e7-f8a9-b0c1d2e3f4a5',
        now(),
        'GP 21-Day Rule',
        'Where a patient is seen by the same general practitioner within twenty-one days subsequent to the initial consultation, the claim should be paid at subsequent consultation fee i.e., code 90051. The system should auto change the tariff (90050) to subsequent consultation (90051). (section 3.7, page 11)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e1f2a3b4-c5d6-e7f8-a9b0-c1d2e3f4a5b6',
        now(),
        'Specialist 60-Day Rule',
        'The initial consultation tariffs can only be claimed once every sixty days. Where another initial consultation claim is submitted within 60 days the claim should be paid at subsequent tariff rate. (section 3.8, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f2a3b4c5-d6e7-f8a9-b0c1-d2e3f4a5b6c7',
        now(),
        'No Surgical Assistant (NSA) Rule',
        'The tariffs with a "no surgical assistant" (NSA) indicator do not require any assistant. Therefore, claims for discipline 088 (modifier 85) should be auto rejected. Any other claims submitted by surgeons for the same procedure codes with modifiers 80 or 82 should be rejected. (section 3.9, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a3b4c5d6-e7f8-a9b0-c1d2-e3f4a5b6c7d8',
        now(),
        'Not a Benefit (NAB) Tariffs',
        'Tariff codes with a "not a benefit" (NAB) indicator are not covered by the Fund and they pay zero. Tariffs should therefore be automatically rejected. (section 3.10, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        '1a982604-1342-4c7f-a417-c0c0b1bac899',
        now(),
        'No Computer Adjudication (NCA)',
        'The NCA adjudication indicators shows that a claim should not be auto assessed by the system but requires manual adjudication by an assessor. The system should route all tariff codes with this indicator for review. (section 3.11, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c5d6e7f8-a9b0-c1d2-e3f4-a5b6c7d8e9f0',
        now(),
        'Outright Duplicate Check',
        'An absolute duplicate claim/s for the same dependent, same service provider, same treatment date and same tariff codes. Such claims are automatically rejected without manual intervention. (section 3.12.1, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd6e7f8a9-b0c1-d2e3-f4a5-b6c7d8e9f0a1',
        now(),
        'Possible Duplicate Check',
        'Claims that are most likely to be duplicates where the dependent is the same, same service provider, same treatment date and same tariff codes but different service providers are flagged and routed for manual adjudication. (section 3.12.2, page 12)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e7f8a9b0-c1d2-e3f4-a5b6-c7d8e9f0a1b2',
        now(),
        'Duplicate Tooth Number Check',
        'Where the same tooth number is used for one or more procedures by the same provider, the system should check the history claims and route such claims for manual adjudication. (section 3.12.7, page 13)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f8a9b0c1-d2e3-f4a5-b6c7-d8e9f0a1b2c3',
        now(),
        'Suspended Accounts due to Non-Payment',
        'Where the member account is suspended due to non-payment of contribution, the claim would be processed but held in the system. Suspended claims should be held for 90 days from the date received and thereafter rejected if contributions remain outstanding. (section 3.13, page 13)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a9b0c1d2-e3f4-a5b6-c7d8-e9f0a1b2c3d4',
        now(),
        'Suspended Accounts due to Other Reasons',
        'Where the member''s account is suspended due to other reasons other than non-payment of contributions (e.g., card loaning, abuse of medicines), the claims should be routed for manual review with an option to override suspension or reject the claims. (section 3.13, page 13)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b0c1d2e3-f4a5-b6c7-d8e9-f0a1b2c3d4e5',
        now(),
        'Once in a Lifetime Procedures',
        'There are tariffs or procedures that are claimable once in a lifetime (e.g., In-vitro fertilisation, hysterectomy). The system should be configured to pay one such tariff code only per dependent. (section 3.14, page 13)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c1d2e3f4-a5b6-c7d8-e9f0-a1b2c3d4e5f6',
        now(),
        'Periodically Claimed Procedures',
        'For procedures or tariffs claimable periodically (e.g., once a year, twice a year), the system should check the previous claim and use the treatment date to determine if the specified period has lapsed. If the period has not lapsed, the claim is rejected. (section 3.15, page 13)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'd2e3f4a5-b6c7-d8e9-f0a1-b2c3d4e5f6a7',
        now(),
        'Access to Bedding at Private Hospitals',
        'Where a member is admitted into a ward that the product does not qualify, the claim is paid at a percentage of the normal tariff award for that ward (e.g. Icare paid at 60% of tariff award if admitted into a two-bedded ward). (section 3.17, page 14)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'e3f4a5b6-c7d8-e9f0-a1b2-c3d4e5f6a7b8',
        now(),
        'Benefit Booking Validity',
        'Benefit booking is valid for 90 days from the date of capture. Any unused funds are reset so that they reflect as unused balances against the respective benefits. (section 3.18, page 14)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'f4a5b6c7-d8e9-f0a1-b2c3-d4e5f6a7b8c9',
        now(),
        'Group Practice Claims (Discipline 050)',
        'For group practices (Discipline 050), the treating provider details must be captured. The system should link assessing rules to the treating provider discipline. If details are missing, the claim is auto-rejected. (section 3.19, page 14)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'a5b6c7d8-e9f0-a1b2-c3d4-e5f6a7b8c9d0',
        now(),
        'Discipline Override',
        'Discipline override is a field required to override a claim during assessing, so that a claim would pay by applying rules of the discipline used for the override. (section 3.20, page 14)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'b6c7d8e9-f0a1-b2c3-d4e5-f6a7b8c9d0e1',
        now(),
        'Foreign Treatment Payment Rules',
        'Unauthorised foreign treatment to be paid at 50% of claimed amount. Authorised treatment to be paid at either 70% or 100% of claimed amount, based on authorisation. All foreign treatment claims routed for manual adjudication. (section 3.21, page 15)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    ),
    (
        'c7d8e9f0-a1b2-c3d4-e5f6-a7b8c9d0e1f2',
        now(),
        'Benefit Allocation and Limits',
        'Claims are paid up to available benefits. Some benefits have monetary sub-limits and claims should deduct from the respective sub-limits first, then from the annual global limit. The system should deduct benefits in real time. (section 5, page 15)',
        0,
        '06e4cf36-3b42-48fd-bdc6-39eb354a3d5d'
    );