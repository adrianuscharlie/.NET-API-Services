# **🚀 .NET API for Cashout OTC Aggregator Services**  

📌 **Version:** `v1.0.1`  
📌 **Author:** Adrianus Charlie H.A.  
📌 **Last Updated:** `14-03-2025`  

## **📖 Table of Contents**  
- [Introduction](#introduction)  
- [Features](#features)  
- [Prerequisites](#prerequisites)  
- [Installation](#installation)  
- [Configuration](#configuration)  
- [API Endpoints](#api-endpoints)  
- [Database Schema](#database-schema)  
- [Error Handling](#error-handling)  
- [Deployment](#deployment)  


## **📌 Introduction**  
This API provides services for cashout transactions at the counter and handles reversal transactions, built with .NET Core 8. It acts as an aggregator between Client APIs and Partner APIs, enabling seamless many-to-many transaction processing. Each partner can have customizable transaction handling based on their specific requirements. The API supports different and shared approaches for handling Cashout and Reversal transactions.


## ✨ Features
- ✅ Cashout Transactions – Allows processing cashout requests with a partner-specific approach.
- ✅ Reversal Transactions – Enables cancellation of cashout transactions due to issues such as token expiration.
- ✅ JWT Authentication – Supports B2B token authentication following ASPI/Bank Indonesia specifications.
- ✅ Logging & Error Handling – Implements structured logging with Serilog for better traceability and debugging.
- ✅ Caching B2B token - Implements basic caching using RedisDB that runs in Docker Container


## **⚙️ Prerequisites**  
Ensure you have the following installed before running the project:  
- **.NET SDK `8.0.0`** 
- **MYSQL** *(if applicable)*  
- **Docker** *(for Redis and API Deployment)*  

---

## **📦 Installation**  
Clone the repository and install dependencies:  
```sh
git clone https://github.com/adrianuscharlie/.NET-Core-WebAPI-CashoutServices.git
cd NET-Core-WebAPI-CashoutServices
dotnet restore
```

---

## **⚙️ Configuration**  
The detailed configuration can be see through the appsettings.json


---

## **📌 API Endpoints**  
| Method | Endpoint | Description 
|--------|---------|-------------
| `POST` | `/api/Cashout` | Cashout Services 
| `POST` | `/api/Reversal` | Reversal Services


### Cashout Services
#### Request:
```json
{
  "cacode": "C00001",
  "otp": "123456",
  "partnerID": "000102",
  "customerNumber": "",
  "trxType": "CASHOUT",
  "amount": "100000",
  "detail": ""
}
```
#### Response:
```json
{
    "responseCode": "00",
    "responseMessage": "SUKSES",
    "originalReferenceNo": "C0000120250309223451",
    "referenceNo": "Gopay-C0000120250309223451",
    "customerNumber": "",
    "transactionDate": "2025-03-09 22:34:53",
    "additionalInfo": {
        "merchantId": "C00001",
        "merchantName": "INDONESIA C00001",
        "externalStoreId": "",
        "branchId": "",
        "terminalId": ""
    }
}
```

### Reversal Services
#### Request:
```json
{
  "cacode": "C00001",
  "otp": "512254",
  "partnerID": "000102",
  "customerNumber": "",
  "trxType": "REVERSAL",
  "amount": "400000",
  "detail": "C0000120250309221654"
}
```
#### Response:
```json
{
    "responseCode": "00",
    "responseMessage": "Reversal Success",
    "originalReferenceNo": "C0000120250309231528",
    "referenceNo": "OK",
    "customerNumber": "",
    "transactionDate": "2025-03-09 23:16:44",
    "additionalInfo": {
        "trxID": "C0000120250309231528",
        "token": "748158",
        "amount": "900000",
        "noHp": ""
    }
}
```

## **🗂 Database Schema**  
Database schema and dummy data for testing are available on **cashoutservices.sql** inside the DB folder. You can use mysql or XAMPP and import the database using that sql file.


## **⚠️ Error Handling**  
This API follows structured error handling using HTTP status codes:  

| Status Code | Meaning |
|------------|---------|
| `200 OK` | Successful request |
| `400 Bad Request` | Invalid input |
| `401 Unauthorized` | Invalid authentication token |
| `500 Internal Server Error` | Unexpected server error |

---

## **🚀 Deployment**  
### **Using Docker**  
#### **Running the CashoutServices**
```sh
docker compose up --build -d
```

#### **Running Redis Services**
``sh
docker run --name redis-container -d -p 6379:6379 redis
``
### **Running the PartnerAPI Services**
run the .exe (in Windows) or .dll (in Linux) in the bin/Debug folder

