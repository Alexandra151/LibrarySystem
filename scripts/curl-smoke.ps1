# ==== LibrarySystem SMOKE ====
# Jeśli Twój Kestrel nasłuchuje na innym porcie HTTP, podmień 5016 poniżej
param([string]$Base = "http://localhost:5016")

# Wymagany przez middleware nagłówek (dla /api/*)
$headers = @{ "X-Client-Name" = "smoke" }

function Call {
    param(
        [Parameter(Mandatory=$true)][ValidateSet("GET","POST","PUT","PATCH","DELETE")] [string]$Method,
        [Parameter(Mandatory=$true)] [string]$Path,
        [Parameter(Mandatory=$false)] $Body
    )

    $uri = "$Base$Path"

    if ($null -ne $Body) {
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers `
               -ContentType "application/json" -Body ($Body | ConvertTo-Json -Depth 6)
    } else {
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
    }
}

Write-Host "1) Swagger JSON..." -ForegroundColor Cyan
try {
    Invoke-RestMethod -Uri "$Base/swagger/v1/swagger.json" -Method GET | Out-Null
    Write-Host "OK" -ForegroundColor Green
} catch {
    Write-Warning "Swagger JSON niedostępny: $($_.Exception.Message)"
}

Write-Host "2) Public GET /api/Authors..." -ForegroundColor Cyan
try {
    Invoke-RestMethod -Uri "$Base/api/Authors" -Method GET -Headers $headers | Out-Null
    Write-Host "OK" -ForegroundColor Green
} catch {
    Write-Warning "GET /api/Authors błąd: $($_.Exception.Message)"
}

Write-Host "3) Login admin -> JWT..." -ForegroundColor Cyan
$login = Call POST "/api/Auth/login" @{ username = "admin"; password = "admin123" }
$token = $login.token
if (-not $token) { throw "Brak pola 'token' w odpowiedzi logowania." }
$headers["Authorization"] = "Bearer $token"
Write-Host "Token OK" -ForegroundColor Green

Write-Host "4) Create Author..." -ForegroundColor Cyan
$author = Call POST "/api/Authors" @{ name = "Smoke Author " + (Get-Random) }
$authorId = $author.id
Write-Host "AuthorId = $authorId" -ForegroundColor Green

Write-Host "5) Create Book..." -ForegroundColor Cyan
$book = Call POST "/api/Books" @{
    title           = "Smoke Book " + (Get-Random)
    authorId        = $authorId
    copiesTotal     = 3
    copiesAvailable = 3
}
$bookId = $book.id
Write-Host "BookId = $bookId" -ForegroundColor Green

Write-Host "6) Create Loan..." -ForegroundColor Cyan
$loan = Call POST "/api/Loans" @{ bookId = $bookId; days = 7 }
$loanId = $loan.id
Write-Host "LoanId = $loanId" -ForegroundColor Green

Write-Host "7) Return Loan..." -ForegroundColor Cyan
Call PATCH "/api/Loans/$loanId/return" | Out-Null
Write-Host "Returned" -ForegroundColor Green

Write-Host "8) GraphQL books..." -ForegroundColor Cyan
$gql = @{ query = "query { books { id title authorId copiesTotal copiesAvailable } }" }
$gqlResp = Invoke-RestMethod -Uri "$Base/graphql" -Method POST -Headers $headers `
          -ContentType "application/json" -Body ($gql | ConvertTo-Json -Depth 6)
$gqlResp.data.books | Format-Table

Write-Host "=== DONE ===" -ForegroundColor Green
