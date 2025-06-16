# Cimas Claims Switch API Documentation

## 1. Overview

The Cimas Claims Switch API provides a programmatic interface for healthcare service providers to validate member details, submit medical claims, reverse claims, and retrieve claim history. The API follows a standard RESTful architecture, using JSON for all data interchange and JWT for authentication.

### Base URL

All API endpoints described in this documentation are relative to the following base URL for the testing environment:

`https://claimsswitch-test.cimas.co.zw`

### General Workflow

A typical interaction with the API follows these steps:
1.  **Authentication:** Obtain an `access_token` by providing your credentials. Use this token for all subsequent requests. Refresh the token when it expires.
2.  **Member Lookup:** (Optional but Recommended) Fetch member details using their membership number to verify eligibility and populate claim data.
3.  **Post a Claim:** Submit a new claim for services or products rendered.
4.  **Reverse a Claim:** If a claim was submitted in error, it can be reversed using the transaction number from the original claim response.
5.  **Upload Prescription/Document:** (Optional) Attach a supporting document or image to a submitted claim.

---

## 2. Authentication

The API uses JSON Web Tokens (JWT) for authentication. Every request to a protected endpoint must include an `Authorization` header with a valid bearer token.

`Authorization: Bearer <your_access_token>`

### Step 1.A: Get Access & Refresh Token

This endpoint authenticates your credentials and returns an `access_token` and a `refresh_token`.

*   **Method:** `POST`
*   **Endpoint:** `/login/`
*   **Details:**
    *   The `access_token` has a short lifespan (5 minutes).
    *   The `refresh_token` has a longer lifespan (24 hours) and is used to get a new `access_token` without re-submitting credentials.

**Request Body:**

```json
{
  "acc_name": "YOUR_ACCOUNT_NAME",
  "password": "YOUR_PASSWORD"
}
```

**Success Response (200 OK):**

```json
{
    "data": {
        "notify": null,
        "tokens": {
            "access": "eyJhbGciOiJIUzI1NiIs...",
            "refresh": "eyJ0eXAiOiJKV1QiLCJ..."
        }
    },
    "dataList": [],
    "message": "Login Successfully",
    "status": 200
}
```

### Step 1.B: Refresh Access Token

Use the `refresh_token` from the login response to obtain a new `access_token`.

*   **Method:** `POST`
*   **Endpoint:** `/jwt/refresh/`

**Request Body:**

```json
{
  "refresh": "YOUR_REFRESH_TOKEN"
}
```

**Success Response (200 OK):**

```json
{
  "access": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

## 3. API Endpoints

### Step 2: Get Member Details

Fetches a member's details, including their status, insurer, and personal information.

*   **Method:** `GET`
*   **Endpoint:** `/claims/2cana/member/details/{member_number-suffix}/{insurer}/`
*   **Headers:** `Authorization: Bearer <your_access_token>`

**Path Parameters:**

| Parameter              | Type   | Description                                           | Example      |
| ---------------------- | ------ | ----------------------------------------------------- | ------------ |
| `member_number-suffix` | string | The member's full membership number including the suffix. | `11067105-00`|
| `insurer`              | string | The insurer code.                                     | `cimas`      |

**Example Request (cURL):**

```bash
curl --location 'https://claimsswitch-test.cimas.co.zw/claims/2cana/member/details/11067105-00/cimas/' \
--header 'Authorization: Bearer <your_access_token>'
```

**Success Response (200 OK):**
*The response from this endpoint provides essential data for populating the `Post a Claim` request, such as `Insurer.Name`, `Insurer.Code`, `Insurer.FunderCode`, `Personal` details, etc.*

```json
{
    "data": {
        "Product": {
            "Code": 3023,
            "Name": "ESSENTIAL"
        },
        "Currency": "USD",
        "MembershipNumber": 11067105,
        "Insurer": {
            "Code": 31,
            "Name": "HEALTHGUARD",
            "FunderCode": "CIHGZW"
        },
        "CellPhoneNo": "0821111111111",
        "Email": "FAIL@2CANA.CO.ZA",
        "DependantNo": 0,
        "DependantType": "H",
        "IdType": "OTHER",
        "IdNumber": "27-103958-Y-27",
        "FirstNames": "CAULIFLOWER TOAST",
        "Initials": "C",
        "Surname": "MINIVAN",
        "Birthdate": "1974-12-01",
        "Gender": "F",
        "Title": "MS",
        "JoinDate": "1999-06-01",
        "Status": "ACTIVE",
        "BenefitStartDate": "2022-03-01",
        "ManagedCare": false,
        "biometric_verification": false,
        "allow_bypass": false,
        "has_fingerprint": true,
        "list_fpos": [
            "LEFT THUMB",
            "LEFT INDEX",
            "LEFT MIDDLE",
            "LEFT RING",
            "RIGHT THUMB"
        ]
    },
    "dataList": [],
    "message": null,
    "status": 200
}
```

### Step 3: Post a Claim

Submits a full claim containing services and/or products.

*   **Method:** `POST`
*   **Endpoint:** `/claims/create/fullclaim/`
*   **Headers:** `Authorization: Bearer <your_access_token>`

**Request Body Structure:**

The request body is a single JSON object with a root key `Request`.

**Important Data Formatting:**
*   **Amounts & Quantities**: All currency amounts (`GrossAmount`, `NettAmount`) and quantities (`Quantity`) must be sent as integers, with the last two digits representing the decimal part.
    *   Example: An amount of **$27.50** should be sent as `2750`.
    *   Example: A quantity of **1.00** should be sent as `100`.
*   **`Outpatient` Key**: This field in `ClaimHeader` accepts one of two string values: `"OutPatient"` or `"InPatient"`.
*   **`Practice` Key**: The `Practice` array can accept objects representing different practice roles. Valid `Name` values include: `"AdmittingPractice"`, `"DischargingPractice"`, `"PrescribingPractice"`, `"ReferringPractice"`, and `"TreatingPractice"`.

**Example Request Body (Products Only):**

```json
{
    "Request": {
        "Transaction": {
            "VersionNumber": "2.1",
            "Type": "CL",
            "DestinationCode": "CIMAS",
            "SoftwareIdentifier": "CIMAS",
            "DateTime": 20240229113314,
            "TestClaimIndicator": "Y",
            "CountryISOCode": "ZM"
        },
        "Provider": {
            "Role": "SP",
            "PracticeNumber": "0965804",
            "PracticeName": "CIMAS BORROWDALE OFFICE PARK PHARMACY"
        },
        "Member": {
            "MedicalSchemeNumber": 11067105,
            "MedicalSchemeName": "HEALTHGUARD",
            "Currency": "USD"
        },
        "Patient": {
            "DependantCode": 0,
            "NewBornIndicator": "N",
            "Personal": {
                "Title": "MS",
                "Surname": "MINIVAN",
                "FirstName": "CAULIFLOWER TOAST",
                "Initials": "C",
                "Gender": "F",
                "IDNumber": "27-103958-Y-27",
                "DateOfBirth": "19990601"
            }
        },
        "ClaimHeader": {
            "ClaimNumber": "TESTPHARM02",
            "ClaimDateTime": 20240229113314,
            "TotalServices": 0,
            "TotalProducts": 3,
            "WhomToPay": "P",
            "Outpatient": "OutPatient",
            "InHospitalIndicator": "N",
            "TotalValues": {
                "GrossAmount": 2700,
                "NettAmount": 2700,
                "PatientPayAmount": 0
            }
        },
        "Practice": [
            {
                "PCNSNumber": "0903698",
                "Name": "ReferringPractice"
            }
        ],
        "Products": [
            {
                "ProductReference1": "1",
                "Code": "20363",
                "Description": "FORTUM INJECTION",
                "System": "NAPPI",
                "Quantity": 1000,
                "SubTotalValues": {
                    "GrossAmount": 1500,
                    "NettAmount": 1500
                },
                "TotalValues": {
                    "GrossAmount": 1500,
                    "NettAmount": 1500
                }
            },
            {
                "ProductReference1": "2",
                "Code": "4258",
                "Description": "ALERID (CETIRIZINE) 10MG TABLETS",
                "System": "NAPPI",
                "Quantity": 1000,
                "SubTotalValues": {
                    "GrossAmount": 700,
                    "NettAmount": 700
                },
                "TotalValues": {
                    "GrossAmount": 700,
                    "NettAmount": 700
                }
            },
            {
                "ProductReference1": "3",
                "Code": "2174",
                "Description": "STOPAYNE TABLETS",
                "System": "NAPPI",
                "Quantity": 2800,
                "SubTotalValues": {
                    "GrossAmount": 500,
                    "NettAmount": 500
                },
                "TotalValues": {
                    "GrossAmount": 500,
                    "NettAmount": 500
                }
            }
        ]
    }
}
```

**Success Response (201 Created):**
*The `TransactionResponse.Number` is critical. You must store it to reverse the claim or upload documents later.*

```json
{
    "data": {
        "Response": {
            "TransactionResponse": {
                "Type": "CL",
                "Number": "TN-20240301091546-3089", // <-- IMPORTANT!
                "ClaimNumber": "TESTPHARM02",
                "DateTime": "2024-02-29T11:33:14",
                "SubmittedBy": "0965804",
                "Reversed": false,
                "DateReversed": null
            },
            // ... other response details ...
            "ClaimHeaderResponse": {
                "ResponseCode": "HELD FOR REVIEW",
                // ...
            },
            "ProductResponse": [
                {
                    "Number": "1",
                    "Code": "20363",
                    // ...
                    "Message": {
                        "Type": "HELD FOR REVIEW",
                        "Code": null,
                        "Description": "CLAIM UNDER REVIEW"
                    }
                }
            ]
        }
    },
    "dataList": [],
    "developer_msg": null,
    "status": 201
}
```

### Step 4: Reverse a Claim

Reverses a previously submitted claim.

*   **Method:** `POST`
*   **Endpoint:** `/claims/reverse`
*   **Headers:** `Authorization: Bearer <your_access_token>`

**Request Body:**
The `transaction_number` is the `Number` returned in the `TransactionResponse` of the Post Claim request.

```json
{
  "transaction_number": "TN-20240301091546-3089"
}
```

**Success Response (200 OK):**

```json
{
    "data": null,
    "dataList": [],
    "message": "reversed successfully",
    "status": 200
}
```

### Step 5: Get Past Claims

Retrieves a list of past claims submitted by a specific practice.

*   **Method:** `GET`
*   **Endpoint:** `/claims/past/claims/{practice_number}/`
*   **Headers:** `Authorization: Bearer <your_access_token>`

**Path Parameters:**

| Parameter         | Type   | Description                      | Example |
| ----------------- | ------ | -------------------------------- | ------- |
| `practice_number` | string | The practice number of the provider. | `0965804` |

**Example Request (cURL):**

```bash
curl --location 'https://claimsswitch-test.cimas.co.zw/claims/past/claims/0965804/' \
--header 'Authorization: Bearer <your_access_token>'
```

**Success Response (200 OK):**
*The response is a paginated list containing full claim details.*

```json
{
    "count": 1,
    "next": null,
    "previous": null,
    "result": [
        {
            "Response": {
                "TransactionResponse": {
                    "Type": null,
                    "Number": "TN-20240301091546-3089",
                    "ClaimNumber": "TESTPHARM02",
                    "DateTime": "2024-03-01T09:15:46",
                    "SubmittedBy": "propharm_admin",
                    "Reversed": false,
                    "DateReversed": null
                },
                // ... full claim details ...
            }
        }
    ]
}
```

### Step 6: Upload Document/Prescription

Uploads a supporting document (e.g., a prescription scan) and links it to a claim.

*   **Method:** `POST`
*   **Endpoint:** `/claims/upload-file/{transaction_number}/{channel}`
*   **Headers:**
    *   `Authorization: Bearer <your_access_token>`
    *   `Content-Type: multipart/form-data`
*   **Description:** This endpoint accepts file uploads. The body of the request should be `multipart/form-data` containing the file.

**Path Parameters:**

| Parameter            | Type   | Description                                                              | Example                    |
| -------------------- | ------ | ------------------------------------------------------------------------ | -------------------------- |
| `transaction_number` | string | The transaction number from the original claim response.                 | `TN-20240301091546-3089` |
| `channel`            | string | The channel or type of document being uploaded.                          | `prescription`             |


---

## 4. Error Handling and Status Codes

### General Error Response

Most API errors (excluding token refresh errors) will return a JSON object in the following format:

```json
{
    "message": "A user-friendly error message.",
    "developer_msg": "A more technical error message for debugging.",
    "status": XXX
}
```

### Authentication Error Response

An error during token refresh (e.g., an expired or invalid refresh token) will have a different structure:

```json
{
    "detail": "Token is invalid or expired",
    "code": "token_not_valid"
}
```

### HTTP Status Codes

| Code  | Meaning                                        | Description                                                                 |
| ----- | ---------------------------------------------- | --------------------------------------------------------------------------- |
| `200` | **Success**                                    | The request was successful (e.g., for GET requests, token refresh, reversal). |
| `201` | **Created**                                    | The resource was successfully created (e.g., for a new claim submission).   |
| `400` | **Bad Request**                                | The request was malformed, such as having missing or invalid parameters.    |
| `403` | **Forbidden / Invalid Credential**             | The `access_token` is invalid, expired, or not provided.                    |
| `500` | **Internal Server Error**                      | An unexpected error occurred on the server.                                 |