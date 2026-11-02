# RaceDay | Part 1 API Endpoint Plan

The table includes all six required columns and corresponds to the implemented API controllers in Part 2. Routes are relative to the API base URL. Role permissions are enforced by server-side sessions: 401 means login required and 403 means the role or ownership is incorrect. `Any` means either authenticated role. Events and categories remain publicly browseable.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Register a user | Public | fullName,email,password,role,phone,city | 201 created; 400 validation; 409 duplicate email |
| POST | /api/auth/login | Create server session | Public | email,password | 200 user session; 401 invalid credentials |
| GET | /api/auth/session | Current login state | Any | None | 200 session; 401 unauthenticated |
| POST | /api/auth/logout | Clear login session | Any | None | 204 success; 401 unauthenticated |
| GET | /api/profile | Read own profile | Any | None | 200 profile; 401 unauthenticated |
| PUT | /api/profile | Update own profile | Any | fullName,phone,city | 204 updated; 400 validation; 401 unauthenticated |
| POST | /api/profile/picture | Upload own profile image to Blob | Any | multipart file JPG/PNG/WEBP | 200 uploaded; 400 invalid image; 503 storage offline |
| GET | /api/profile/picture | Stream own image from Blob | Any | None | 200 image; 401 unauthenticated; 404 no image |
| GET | /api/events | List/filter events | Public | None (query: search,type,upcoming) | 200 event list |
| GET | /api/events/{id} | Get one event | Public | None | 200 event; 404 missing |
| GET | /api/events/mine | List events organiser owns | Organiser | None | 200 list; 401/403 unauthorised |
| POST | /api/events | Create race | Organiser | name,description,dateUtc,location,distanceKm,eventType | 201 event; 400 validation; 401/403 |
| PUT | /api/events/{id} | Update owned race | Organiser | name,description,dateUtc,location,distanceKm,eventType | 204 success; 403 not owned; 404 missing |
| DELETE | /api/events/{id} | Delete unentered race | Organiser | None | 204 deleted; 409 has entries; 403/404 |
| POST | /api/events/{id}/banner | Upload owned race banner to Blob | Organiser | multipart file | 200 uploaded; 403/404; 503 offline |
| GET | /api/events/{id}/banner | Stream public event banner | Public | None | 200 image; 404 missing; 503 offline |
| GET | /api/events/{id}/categories | List race categories | Public | None | 200 categories; 404 missing race |
| POST | /api/events/{id}/categories | Create category for race | Organiser | name,minAge,maxAge,distanceKm | 201 category; 400 invalid; 403; 409 duplicate |
| PUT | /api/categories/{id} | Edit category | Organiser | name,minAge,maxAge,distanceKm | 204 success; 400; 403; 404; 409 |
| DELETE | /api/categories/{id} | Delete unused category | Organiser | None | 204 success; 403; 404; 409 has enrolments |
| POST | /api/enrolments | Enter race in a category | Participant | eventId,categoryId | 201 entry; 400 wrong category; 409 duplicate/past |
| GET | /api/enrolments/mine | Participant sees own entries | Participant | None | 200 entries; 401/403 |
| GET | /api/events/{id}/enrolments | Organiser views event entries | Organiser (owner) | None | 200 entries; 403 not owned; 404 missing |
| PUT | /api/enrolments/{id}/status | Update entry Pending/Confirmed | Organiser (owner) | status | 204 success; 400 invalid; 403; 404 |
| POST | /api/results | Publish/update result for past race | Organiser (owner) | enrolmentId,finishTimeSeconds,position,totalFinishers | 200 saved; 400 invalid; 409 race not finished |
| GET | /api/results/mine | Participant reads own results | Participant | None | 200 results; 401/403 |
| GET | /api/events/{id}/results | Organiser reads own event results | Organiser (owner) | None | 200 results; 403; 404 |
| GET | /health | Health probe | Public | None | 200 healthy |

## API design choices

- A Participant may enter an upcoming event once, in a category belonging to that event.
- An Organiser can modify only events they own; protected operations verify ownership in the API and not just the MVC interface.
- Results can be published only for confirmed entries after the event date and are linked one-to-one with enrolments.
- Images are stored by the **API** using Azure Blob Storage. The MVC application never connects to Azure directly.
- The `GET /health` endpoint is additional infrastructure support.
