import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InvoiceService } from '../../services/invoice.service';
import { PaymentService } from '../../services/payment.service';
import { Invoice, OutstandingBalance, RecordPayment } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, NotificationBellComponent],
  templateUrl: './invoices.component.html'
})
export class InvoicesComponent implements OnInit {
  invoices: Invoice[] = [];
  outstandingBalances: OutstandingBalance[] = [];
  loading = false;
  error = '';
  success = '';
  activeTab: 'invoices' | 'outstanding' | 'payments' = 'invoices';
  payments: any[] = [];
  loadingPayments = false;

  showGenerateForm = false;
  showPaymentForm = false;
  selectedInvoice: Invoice | null = null;

  generateForm: FormGroup;
  paymentForm: FormGroup;

  paymentMethods = [
    { value: 0, label: 'Cash' },
    { value: 1, label: 'Bank Transfer' },
    { value: 2, label: 'Cheque' },
    { value: 3, label: 'Mobile Money' }
  ];

  currentYear = new Date().getFullYear();
  months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' },
    { value: 3, label: 'March' }, { value: 4, label: 'April' },
    { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' },
    { value: 9, label: 'September' }, { value: 10, label: 'October' },
    { value: 11, label: 'November' }, { value: 12, label: 'December' }
  ];

  constructor(
    private invoiceService: InvoiceService,
    private paymentService: PaymentService,
    private fb: FormBuilder
  ) {
    this.generateForm = this.fb.group({
      month: [new Date().getMonth() + 1, Validators.required],
      year: [this.currentYear, Validators.required]
    });

    this.paymentForm = this.fb.group({
      amountPaid: ['', [Validators.required, Validators.min(0.01)]],
      paymentDate: [new Date().toISOString().split('T')[0], Validators.required],
      paymentMethod: [0, Validators.required],
      referenceNumber: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadInvoices();
    this.loadOutstandingBalances();
  }

  setTab(tab: 'invoices' | 'outstanding' | 'payments'): void {
    this.activeTab = tab;
    if (tab === 'payments' && this.payments.length === 0) {
      this.loadAllPayments();
    }
  }

  loadInvoices(): void {
    this.loading = true;
    this.invoiceService.getAll().subscribe({
      next: (data) => { this.invoices = data; this.loading = false; },
      error: () => { this.error = 'Failed to load invoices'; this.loading = false; }
    });
  }

  loadOutstandingBalances(): void {
    this.invoiceService.getOutstandingBalances().subscribe({
      next: (data) => this.outstandingBalances = data
    });
  }

  loadAllPayments(): void {
    this.loadingPayments = true;
    this.paymentService.getAll().subscribe({
      next: (data) => { this.payments = data; this.loadingPayments = false; },
      error: () => { this.loadingPayments = false; }
    });
  }

  openGenerate(): void {
    this.showGenerateForm = true;
    this.error = '';
    this.success = '';
  }

  cancelGenerate(): void {
    this.showGenerateForm = false;
  }

  submitGenerate(): void {
    if (this.generateForm.invalid) return;
    const v = this.generateForm.value;
    this.invoiceService.generateMonthly(+v.month, +v.year).subscribe({
      next: (invoices) => {
        this.success = `Generated ${invoices.length} invoice(s) for ${this.months[+v.month - 1].label} ${v.year}`;
        this.showGenerateForm = false;
        this.loadInvoices();
        this.loadOutstandingBalances();
      },
      error: (err) => this.error = err.error?.message || 'Generation failed'
    });
  }

  openPayment(invoice: Invoice): void {
    this.selectedInvoice = invoice;
    this.paymentForm.patchValue({
      amountPaid: invoice.outstandingBalance,
      paymentDate: new Date().toISOString().split('T')[0],
      paymentMethod: 0,
      referenceNumber: '',
      notes: ''
    });
    this.showPaymentForm = true;
    this.error = '';
    this.success = '';
  }

  cancelPayment(): void {
    this.showPaymentForm = false;
    this.selectedInvoice = null;
  }

  submitPayment(): void {
    if (this.paymentForm.invalid || !this.selectedInvoice) return;
    const v = this.paymentForm.value;
    const dto: RecordPayment = {
      invoiceId: this.selectedInvoice.id,
      amountPaid: +v.amountPaid,
      paymentDate: v.paymentDate,
      paymentMethod: +v.paymentMethod,
      referenceNumber: v.referenceNumber,
      notes: v.notes
    };
    this.paymentService.record(dto).subscribe({
      next: () => {
        this.success = 'Payment recorded successfully';
        this.showPaymentForm = false;
        this.selectedInvoice = null;
        this.loadInvoices();
        this.loadOutstandingBalances();
        if (this.activeTab === 'payments') this.loadAllPayments();
      },
      error: (err) => this.error = err.error?.message || 'Payment failed'
    });
  }

  markOverdue(): void {
    this.invoiceService.markOverdue().subscribe({
      next: () => {
        this.success = 'Overdue invoices marked';
        this.loadInvoices();
      },
      error: (err) => this.error = err.error?.message || 'Failed'
    });
  }
}
