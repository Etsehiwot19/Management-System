export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  role: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface Unit {
  id: number;
  floorNumber: number;
  unitNumber: string;
  unitType: string;
  areaSqMeters: number;
  monthlyRent: number;
  status: string;
  description: string;
}

export interface CreateUnit {
  floorNumber: number;
  unitNumber: string;
  unitType: number;
  areaSqMeters: number;
  monthlyRent: number;
  description: string;
}

export interface Tenant {
  id: number;
  organizationName: string;
  tinNumber: string;
  phone: string;
  businessType: string;
  contactPersonName: string;
  contactEmail: string;
  isActive: boolean;
}

export interface CreateTenant {
  organizationName: string;
  tinNumber: string;
  phone: string;
  businessType: number;
  contactPersonName: string;
  contactEmail: string;
}

export interface Lease {
  id: number;
  unitId: number;
  unitNumber: string;
  tenantId: number;
  tenantName: string;
  startDate: string;
  endDate: string;
  monthlyRent: number;
  depositAmount: number;
  status: string;
  terminationReason: string;
}

export interface CreateLease {
  unitId: number;
  tenantId: number;
  startDate: string;
  endDate: string;
  monthlyRent: number;
  depositAmount: number;
}

export interface Invoice {
  id: number;
  leaseId: number;
  invoiceNumber: string;
  amountDue: number;
  amountPaid: number;
  outstandingBalance: number;
  dueDate: string;
  issueDate: string;
  status: string;
  periodMonth: number;
  periodYear: number;
  tenantName: string;
  unitNumber: string;
}

export interface Payment {
  id: number;
  invoiceId: number;
  invoiceNumber: string;
  amountPaid: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber: string;
  notes: string;
  tenantName: string;
  unitNumber: string;
}

export interface RecordPayment {
  invoiceId: number;
  amountPaid: number;
  paymentDate: string;
  paymentMethod: number;
  referenceNumber: string;
  notes: string;
}

export interface OutstandingBalance {
  leaseId: number;
  tenantId: number;
  tenantName: string;
  unitNumber: string;
  totalDue: number;
  totalPaid: number;
  outstandingBalance: number;
  overdueInvoiceCount: number;
}

export interface RevenueReportItem {
  year: number;
  month: number;
  amount: number;
}

export interface DashboardSummary {
  totalUnits: number;
  availableUnits: number;
  occupiedUnits: number;
  activeLeases: number;
  activeTenants: number;
  revenueThisMonth: number;
  outstandingBalance: number;
  overdueInvoiceCount: number;
  revenue: RevenueReportItem[];
}

export interface ReportFilters {
  fromDate: string;
  toDate: string;
  expiryWithinDays: number;
}

export interface ReportsResponse {
  occupancy: {
    totalUnits: number;
    occupiedUnits: number;
    availableUnits: number;
    occupancyRate: number;
  };
  revenue: RevenueReportItem[];
  arrears: {
    invoiceId: number;
    invoiceNumber: string;
    tenantName: string;
    unitNumber: string;
    dueDate: string;
    amountDue: number;
    amountPaid: number;
    outstandingBalance: number;
  }[];
  leaseExpiries: {
    leaseId: number;
    tenantName: string;
    unitNumber: string;
    endDate: string;
    monthlyRent: number;
  }[];
  documentExpiries: {
    documentId: number;
    tenantName: string;
    documentType: string;
    expiryDate: string;
    isVerified: boolean;
  }[];
}

export interface NotificationItem {
  id: number;
  title: string;
  message: string;
  notificationType: string;
  isRead: boolean;
  createdAt: string;
}
