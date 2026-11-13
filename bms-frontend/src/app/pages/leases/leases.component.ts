import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeaseService } from '../../services/lease.service';
import { TenantService } from '../../services/tenant.service';
import { UnitService } from '../../services/unit.service';
import { Lease, Tenant, Unit } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-leases',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, NotificationBellComponent],
  templateUrl: './leases.component.html'
})
export class LeasesComponent implements OnInit {
  leases: Lease[] = [];
  tenants: Tenant[] = [];
  units: Unit[] = [];
  loading = false;
  error = '';
  success = '';
  showCreateForm = false;
  showRenewForm = false;
  showTerminateForm = false;
  selectedLease: Lease | null = null;
  createForm: FormGroup;
  renewForm: FormGroup;
  terminateForm: FormGroup;

  constructor(
    private leaseService: LeaseService,
    private tenantService: TenantService,
    private unitService: UnitService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      unitId: ['', Validators.required],
      tenantId: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      monthlyRent: ['', [Validators.required, Validators.min(1)]],
      depositAmount: ['', [Validators.required, Validators.min(0)]]
    });
    this.renewForm = this.fb.group({
      newEndDate: ['', Validators.required],
      newMonthlyRent: ['', [Validators.required, Validators.min(1)]]
    });
    this.terminateForm = this.fb.group({
      terminationReason: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.leaseService.getAll().subscribe({
      next: (data) => {
        this.leases = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load leases';
        this.loading = false;
      }
    });
    this.tenantService.getAll().subscribe({
      next: (data) => this.tenants = data.filter(t => t.isActive)
    });
    this.unitService.getAll().subscribe({
      next: (data) => this.units = data.filter(u => u.status === 'Available')
    });
  }

  openCreate(): void {
    this.createForm.reset();
    this.unitService.getAll().subscribe({
      next: (data) => this.units = data.filter(u => u.status === 'Available')
    });
    this.showCreateForm = true;
    this.error = '';
    this.success = '';
  }

  cancelCreate(): void { this.showCreateForm = false; }

  submitCreate(): void {
    if (this.createForm.invalid) return;
    const v = this.createForm.value;
    this.leaseService.create({
      unitId: +v.unitId,
      tenantId: +v.tenantId,
      startDate: v.startDate,
      endDate: v.endDate,
      monthlyRent: +v.monthlyRent,
      depositAmount: +v.depositAmount
    }).subscribe({
      next: () => {
        this.success = 'Lease created successfully';
        this.showCreateForm = false;
        this.loadAll();
      },
      error: (err) => this.error = err.error?.message || 'Create failed'
    });
  }

  openRenew(lease: Lease): void {
    this.selectedLease = lease;
    this.renewForm.patchValue({ newEndDate: '', newMonthlyRent: lease.monthlyRent });
    this.showRenewForm = true;
    this.error = '';
    this.success = '';
  }

  cancelRenew(): void { this.showRenewForm = false; this.selectedLease = null; }

  submitRenew(): void {
    if (this.renewForm.invalid || !this.selectedLease) return;
    const v = this.renewForm.value;
    this.leaseService.renew(this.selectedLease.id, {
      newEndDate: v.newEndDate,
      newMonthlyRent: +v.newMonthlyRent
    }).subscribe({
      next: () => {
        this.success = 'Lease renewed successfully';
        this.showRenewForm = false;
        this.selectedLease = null;
        this.loadAll();
      },
      error: (err) => this.error = err.error?.message || 'Renewal failed'
    });
  }

  openTerminate(lease: Lease): void {
    this.selectedLease = lease;
    this.terminateForm.reset();
    this.showTerminateForm = true;
    this.error = '';
    this.success = '';
  }

  cancelTerminate(): void { this.showTerminateForm = false; this.selectedLease = null; }

  submitTerminate(): void {
    if (this.terminateForm.invalid || !this.selectedLease) return;
    this.leaseService.terminate(this.selectedLease.id, this.terminateForm.value).subscribe({
      next: () => {
        this.success = 'Lease terminated';
        this.showTerminateForm = false;
        this.selectedLease = null;
        this.loadAll();
      },
      error: (err) => this.error = err.error?.message || 'Termination failed'
    });
  }

  daysUntilExpiry(endDate: string): number {
    const end = new Date(endDate);
    const today = new Date();
    return Math.ceil((end.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
  }
}
