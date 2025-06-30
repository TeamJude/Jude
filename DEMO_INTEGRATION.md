# Jude Claims Processing Demo Integration

This document explains the real-time integration between the backend AI agents and the frontend for the demo.

## What This Demo Shows

1. **Real Claims Data**: Backend serves actual claims from the database instead of mock data
2. **Live Processing**: Claims move through the processing pipeline in real-time
3. **AI Agent Integration**: Backend agents actually process claims with recommendations
4. **Visual Pipeline**: See claims move from "In Queue" → "In Progress" → "Awaiting Review"

## Key Features Implemented

### Backend Integration
- ✅ Added `/api/claims/internal` endpoint to fetch real claims from database
- ✅ Added `/api/orchestration/demo/process-recent-claims` endpoint for demo processing
- ✅ Enhanced ClaimsService with internal claims methods
- ✅ Added sample claims data to database seeder
- ✅ Connected OrchestrationController with AI agent workflow

### Frontend Integration
- ✅ Created `claimsService` for API communication
- ✅ Added "Process Claims" button that triggers real workflow
- ✅ Added "Load Real Data" button to fetch actual claims
- ✅ Real-time status updates showing processing stages
- ✅ Visual indicators for real vs demo data
- ✅ Enhanced claim modal with agent recommendations

## Demo Flow

1. **Load Existing Claims**: Click "Load Real Data" to see any existing claims in database
2. **Ingest & Process**: Click "Ingest & Process Claims" to trigger the full workflow:
   - **Fetches real claims** from CIMAS API (getPastClaims)
   - **Takes 3 most recent** claims from the response
   - **Saves them to database** with initial pending status
   - **Triggers AI agent processing** through the orchestrator
3. **Watch Live Pipeline**: See claims move through processing stages:
   - **In Queue**: Newly ingested claims waiting to be processed
   - **In Progress**: AI agents actively analyzing (with live progress indicators)
   - **Awaiting Review**: Completed analysis ready for human review
4. **View Results**: Click the eye icon on processed claims to see AI recommendations and analysis

## Technical Implementation

### API Endpoints
```
GET /api/claims/internal?take=10                    # Fetch existing claims from database
GET /api/claims/past-claims/{practiceNumber}        # Fetch claims from CIMAS API
POST /api/orchestration/demo/ingest-and-process     # Full ingestion + processing workflow
```

### Real Workflow Integration
- **Fetches live claims** from CIMAS API using actual getPastClaims method
- **Takes 3 most recent** claims from the CIMAS response
- **Maps and saves** claims to database with initial state (like background service does)
- **Triggers actual AI agent processing** through the adjudication orchestrator
- **Real agent analysis** with actual recommendations and confidence scores
- **Frontend simulation** for visual progress (since we can't do real-time SignalR yet)

### Data Flow
```
CIMAS API → Ingest Claims → Database → AI Agent Processing → Updated Claims → Frontend Visualization
```

## Sample Claims Data

The database seeder creates 5 sample claims:
- 3 pending claims (for processing demo)
- 2 already processed claims (showing completed analysis)

## Demo Script

1. Start with: "Here's our end-to-end claims processing system"
2. Show: "Load Real Data" to demonstrate existing claims in database
3. Explain: "Now watch as we ingest fresh claims directly from CIMAS and process them"
4. Click: "Ingest & Process Claims" to start the full workflow
5. Narrate: "We're fetching the 3 most recent claims from CIMAS API, saving them to our database, and immediately processing them with our AI agents"
6. Show: New claims appearing in the "In Queue" lane, then moving through processing
7. Highlight: "Notice the real patient names, claim amounts, and transaction numbers from CIMAS"
8. Open: A processed claim to show actual AI agent recommendations and confidence scores
9. Conclude: "This is our complete automated pipeline - from CIMAS ingestion to AI-powered adjudication in minutes, not days"

## Visual Elements

- **Real Data Indicator**: Green chip showing "Real Data" vs "Demo Data"
- **Processing Status**: Live progress bar with current stage
- **Progress Indicators**: Percentage completion for each claim
- **Agent Reasoning**: Live activity logs during processing
- **Recommendations**: AI-generated approve/review decisions
- **Confidence Scores**: Algorithm confidence in recommendations

This integration demonstrates the full power of our AI-powered claims adjudication platform! 