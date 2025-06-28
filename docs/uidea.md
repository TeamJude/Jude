# Claims Management Kanban Interface - UI Design Specification

## Overview
Design a modern, responsive claims management interface using a kanban-style board with three fixed lanes. The interface should feel dynamic and informative, emphasizing real-time processing feedback and clear visual hierarchy.

## Core Layout Structure

### Kanban Board - 3 Fixed Lanes
1. **In Queue** - Claims waiting to be processed
2. **In Progress** - Claims currently being processed by AI agent  
3. **Awaiting Review** - Claims pending human review/decision

**Important**: Unlike traditional kanban, cards CANNOT be dragged between lanes. Lane movement is determined programmatically based on claim processing state.

### Top Navigation Bar
- **Time Range Filter**: Dropdown defaulting to "Today" 
  - Options: Today, Yesterday, Last 7 days, Last 30 days, Custom range
- **Search**: Global search across all claims
- **Stats Summary**: Total claims count per lane with visual indicators

## Claim Card Design

### Card Properties (Primary Display)
```
┌─────────────────────────────────────┐
│ 🔴 HIGH RISK    #TN-20250310141735 │  <- Risk indicator + Transaction #
│                                     │
│ PASSION FRUIT CRIB, Flower          │  <- Patient name (Last, First)
│ Member: 11067105-0                  │  <- Membership number-suffix
│                                     │
│ 💰 $1.51 claimed                    │  <- Claim amount
│ 🏥 Practice: 12345                  │  <- Provider practice number
│                                     │
│ ⏰ 2 hours ago                      │  <- Time since submission/last update
│ 📋 2 products, 1 service           │  <- Item summary
└─────────────────────────────────────┘
```

### Card Visual States
- **In Queue**: Neutral blue border, steady appearance
- **In Progress**: Animated gradient border (pulsing), activity indicator
- **Awaiting Review**: Orange border, attention-grabbing subtle glow
- **Flagged Claims**: Red accent stripe on left edge
- **High Priority**: Crown icon or priority badge

### Card Properties to Display
- **Transaction Number** (e.g., TN-20250310141735)
- **Patient Name** (PatientLastName, PatientFirstName)
- **Membership Number** (MembershipNumber-DependantCode)
- **Claim Amount** (ClaimAmount + Currency)
- **Provider Practice** (ProviderPracticeNumber or ProviderPracticeName)
- **Time Indicator** (relative time since IngestedAt/UpdatedAt)
- **Item Summary** (product/service count from ProductResponse/ServiceResponse)
- **Risk Level** (FraudRiskLevel as color-coded indicator)
- **Status Flags** (IsFlagged, RequiresHumanReview)

## Claim Details Modal

### Modal Header
```
┌─────────────────────────────────────────────────────────┐
│ Claim Details - TN-20250310141735        [❌ Close]    │
│ Status: In Progress 🔄                                  │
│ Risk Level: 🔴 High | Flagged: ⚠️ Yes                  │
└─────────────────────────────────────────────────────────┘
```

### Modal Content Sections

#### 1. Patient & Provider Information
- **Patient**: Full name, ID, DOB, gender, dependant info
- **Provider**: Practice name, number, contact details
- **Member**: Scheme info, membership details

#### 2. Financial Summary
- **Claimed Amount**: Original claim total
- **Approved Amount**: What was approved (if any)
- **Breakdown**: Per item claimed vs approved amounts
- **Currency**: Display currency consistently

#### 3. Claim Items (Expandable Sections)
- **Products**: List with codes, descriptions, quantities, amounts
- **Services**: List with codes, descriptions, amounts
- **Each item**: Show approval status, messages, any rejections

#### 4. AI Processing Section (For In-Progress Claims)
```
┌─────────────────────────────────────────────────────────┐
│ 🤖 AI Agent Processing...                              │
│                                                         │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ [████████████████░░░░░░░░] 78% Complete             │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ 💭 Latest Reasoning:                                   │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ > Analyzing medical codes for consistency...         │ │
│ │ > Cross-referencing member eligibility...            │ │ 
│ │ > Checking fraud patterns...                         │ │
│ │ > Calculating risk score... ⚡                      │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

#### 5. Fraud Analysis (When Applicable)
- **Risk Score**: Visual gauge showing confidence level
- **Indicators**: List of detected fraud indicators
- **Reasoning**: Why each indicator was flagged

#### 6. Timeline/History
- **Ingested**: When claim was received
- **Processing Started**: When AI analysis began  
- **Key Events**: Status changes, reviews, decisions
- **Last Updated**: Most recent activity

#### 7. Action Buttons (Bottom)
- **Approve**: Green button (when ready for decision)
- **Reject**: Red button with reason selection
- **Request More Info**: Yellow button
- **Escalate**: Purple button for complex cases

## Streaming Animation Concepts

### In-Progress Card Animation
1. **Breathing Border**: Gentle pulsing gradient border
2. **Processing Indicator**: Subtle spinner or dot animation
3. **Live Badge**: "LIVE" indicator with gentle glow

### Modal Streaming Section
1. **Progress Bar**: Animated fill showing completion percentage
2. **Typing Effect**: AI reasoning appears character by character
3. **Pulse Effects**: Gentle pulsing on active reasoning lines
4. **Streaming Dots**: "..." animation while waiting for next reasoning step
5. **Success Animations**: Checkmarks appearing as steps complete

### Visual Feedback
- **Real-time Updates**: Use WebSocket/SSE for live updates
- **Smooth Transitions**: When claims move between lanes
- **Loading States**: Skeleton loaders while fetching data
- **Error States**: Clear error indicators with retry options

## Technical Considerations

### State Management
- Claims should auto-refresh based on status changes
- Real-time updates for in-progress claims
- Optimistic updates for better UX

### Performance
- Virtual scrolling for large claim lists
- Lazy loading of modal content
- Efficient WebSocket connection management

### Accessibility
- Keyboard navigation for all interactions
- Screen reader friendly labels
- High contrast mode support
- Focus management in modals

### Responsive Design
- Mobile-first approach
- Cards stack vertically on mobile
- Modal becomes full-screen on mobile
- Touch-friendly interaction areas

## Color Scheme Suggestions
- **In Queue**: Cool blue (#3B82F6)
- **In Progress**: Vibrant purple (#8B5CF6) with animation
- **Awaiting Review**: Warm orange (#F59E0B)
- **High Risk**: Alert red (#EF4444)
- **Medium Risk**: Warning yellow (#F59E0B) 
- **Low Risk**: Safe green (#10B981)
- **Background**: Clean whites/grays for contrast

## Animation Timing
- **Card transitions**: 200-300ms ease-out
- **Modal open/close**: 250ms ease-in-out
- **Progress bar**: 1-2s duration for completions
- **Typing effect**: 30-50ms per character
- **Pulse animations**: 2s cycle, infinite

This interface should feel like a living, breathing system that provides transparency into the AI processing pipeline while maintaining a clean, professional appearance suitable for healthcare/insurance workflows.
