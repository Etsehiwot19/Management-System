import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UnitService } from '../../services/unit.service';
import { Unit } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, NotificationBellComponent],
  templateUrl: './units.component.html'
})
export class UnitsComponent implements OnInit {
  units: Unit[] = [];
  loading = false;  
  error = '';
  success = '';
  showForm = false;
  editingUnit: Unit | null = null;
  unitForm: FormGroup;

  unitTypes = [
    { value: 0, label: 'Shop' },
    { value: 1, label: 'Office' }
  ];

  constructor(private unitService: UnitService, private fb: FormBuilder) {
    this.unitForm = this.fb.group({
      floorNumber: [1, [Validators.required, Validators.min(1)]],
      unitNumber: ['', Validators.required],
      unitType: [0, Validators.required],
      areaSqMeters: ['', [Validators.required, Validators.min(1)]],
      monthlyRent: ['', [Validators.required, Validators.min(1)]],
      description: ['']
    });
  }

  ngOnInit(): void {
    this.loadUnits();
  }

loadUnits(): void {
  // Show cached data instantly, then refresh in background
  this.units = this.unitService.getCached();
  this.loading = this.units.length === 0;

  this.unitService.getAll().subscribe({
    next: (data) => {
      this.units = data;
      this.loading = false;
    },
    error: () => {
      this.error = 'Failed to load units';
      this.loading = false;
    }
  });
}

  openCreate(): void {
    this.editingUnit = null;
    this.unitForm.reset({ unitType: 0, floorNumber: 1 });
    this.unitForm.get('floorNumber')?.enable();
    this.unitForm.get('unitNumber')?.enable();
    this.showForm = true;
    this.error = '';
    this.success = '';
  }

  openEdit(unit: Unit): void {
    this.editingUnit = unit;
    this.unitForm.patchValue({
      floorNumber: unit.floorNumber,
      unitNumber: unit.unitNumber,
      unitType: this.unitTypes.findIndex(t => t.label === unit.unitType),
      areaSqMeters: unit.areaSqMeters,
      monthlyRent: unit.monthlyRent,
      description: unit.description
    });
    this.unitForm.get('floorNumber')?.disable();
    this.unitForm.get('unitNumber')?.disable();
    this.showForm = true;
    this.error = '';
    this.success = '';
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingUnit = null;
  }

  onSubmit(): void {
    if (this.unitForm.invalid) return;
    this.error = '';
    const value = this.unitForm.getRawValue();

    if (this.editingUnit) {
      const updateDto = {
        unitNumber: value.unitNumber,
        unitType: +value.unitType,
        areaSqMeters: +value.areaSqMeters,
        monthlyRent: +value.monthlyRent,
        description: value.description
      };
      this.unitService.update(this.editingUnit.id, updateDto).subscribe({
        next: () => {
          this.success = 'Unit updated successfully';
          this.cancelForm();
          this.loadUnits();
        },
        error: (err) => this.error = err.error?.message || 'Update failed'
      });
    } else {
      const createDto = {
        floorNumber: +value.floorNumber,
        unitNumber: value.unitNumber,
        unitType: +value.unitType,
        areaSqMeters: +value.areaSqMeters,
        monthlyRent: +value.monthlyRent,
        description: value.description
      };
      this.unitService.create(createDto).subscribe({
        next: () => {
          this.success = 'Unit created successfully';
          this.cancelForm();
          this.loadUnits();
        },
        error: (err) => this.error = err.error?.message || 'Create failed'
      });
    }
  }

  deleteUnit(id: number): void {
    if (confirm('Are you sure you want to delete this unit?')) {
      this.unitService.delete(id).subscribe({
        next: () => {
          this.success = 'Unit deleted';
          this.loadUnits();
        },
        error: (err) => this.error = err.error?.message || 'Delete failed'
      });
    }
  }
}
