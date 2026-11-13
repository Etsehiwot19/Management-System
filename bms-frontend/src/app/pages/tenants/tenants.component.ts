import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TenantService } from '../../services/tenant.service';
import { Tenant } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, NotificationBellComponent],
  templateUrl: './tenants.component.html'
})
export class TenantsComponent implements OnInit {
  tenants: Tenant[] = [];
  loading = false;
  error = '';
  success = '';
  showForm = false;
  editingTenant: Tenant | null = null;
  tenantForm: FormGroup;

  businessTypes = [
    { value: 0, label: 'Retail' },
    { value: 1, label: 'Restaurant' },
    { value: 2, label: 'Office' },
    { value: 3, label: 'Services' },
    { value: 4, label: 'Healthcare' },
    { value: 5, label: 'Education' },
    { value: 6, label: 'Other' }
  ];

  constructor(
    private tenantService: TenantService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.tenantForm = this.fb.group({
      organizationName: ['', Validators.required],
      tinNumber: ['', Validators.required],
      phone: ['', Validators.required],
      businessType: [0, Validators.required],
      contactPersonName: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.error = '';
    this.tenantService.getAll().subscribe({
      next: (data) => {
        this.tenants = [...data];
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err.status === 401
          ? 'Session expired. Please log out and log in again.'
          : 'Failed to load tenants';
        this.cdr.detectChanges();
      }
    });
  }

  openCreate(): void {
    this.editingTenant = null;
    this.tenantForm.reset({ businessType: 0 });
    this.tenantForm.get('tinNumber')?.enable();
    this.showForm = true;
    this.error = '';
    this.success = '';
  }

  openEdit(tenant: Tenant): void {
    this.editingTenant = tenant;
    this.tenantForm.patchValue({
      organizationName: tenant.organizationName,
      tinNumber: tenant.tinNumber,
      phone: tenant.phone,
      businessType: this.businessTypes.findIndex(b => b.label === tenant.businessType),
      contactPersonName: tenant.contactPersonName,
      contactEmail: tenant.contactEmail
    });
    this.tenantForm.get('tinNumber')?.disable();
    this.showForm = true;
    this.error = '';
    this.success = '';
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingTenant = null;
    this.tenantForm.reset({ businessType: 0 });
  }

  onSubmit(): void {
    if (this.tenantForm.invalid) return;
    this.error = '';
    const value = this.tenantForm.getRawValue();

    if (this.editingTenant) {
      this.tenantService.update(this.editingTenant.id, {
        organizationName: value.organizationName,
        phone: value.phone,
        businessType: +value.businessType,
        contactPersonName: value.contactPersonName,
        contactEmail: value.contactEmail
      }).subscribe({
        next: () => {
          this.success = 'Tenant updated successfully';
          this.cancelForm();
          this.loadTenants();
        },
        error: (err) => this.error = err.error?.message || 'Update failed'
      });
    } else {
      this.tenantService.create({
        organizationName: value.organizationName,
        tinNumber: value.tinNumber,
        phone: value.phone,
        businessType: +value.businessType,
        contactPersonName: value.contactPersonName,
        contactEmail: value.contactEmail
      }).subscribe({
        next: () => {
          this.success = 'Tenant created successfully';
          this.cancelForm();
          this.loadTenants();
        },
        error: (err) => this.error = err.error?.message || 'Create failed'
      });
    }
  }

  deactivateTenant(id: number): void {
    if (confirm('Are you sure you want to deactivate this tenant?')) {
      this.tenantService.deactivate(id).subscribe({
        next: () => {
          this.success = 'Tenant deactivated';
          this.loadTenants();
        },
        error: (err) => this.error = err.error?.message || 'Deactivation failed'
      });
    }
  }
}
