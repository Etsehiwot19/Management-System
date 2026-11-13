import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'units',
    loadComponent: () => import('./pages/units/units.component').then(m => m.UnitsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'tenants',
    loadComponent: () => import('./pages/tenants/tenants.component').then(m => m.TenantsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'leases',
    loadComponent: () => import('./pages/leases/leases.component').then(m => m.LeasesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'invoices',
    loadComponent: () => import('./pages/invoices/invoices.component').then(m => m.InvoicesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'reports',
    loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: '/dashboard' }
];
