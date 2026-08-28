import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category, Product, MenuResponse } from '../models/menu.model';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7000/api/Menu';

  // Signals ile Reaktif Durum Yönetimi
  categories = signal<Category[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  activeCategoryId = signal<string | null>(null);
  selectedProductForModal = signal<Product | null>(null);
  searchQuery = signal<string>('');

  // Computed: Canlı arama filtresi uygulanmış kategoriler ve ürünler
  filteredCategories = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const all = this.categories();

    if (!query) return all;

    return all
      .map(cat => ({
        ...cat,
        products: cat.products.filter(p =>
          p.name.toLowerCase().includes(query) ||
          p.description.toLowerCase().includes(query)
        )
      }))
      .filter(cat => cat.products.length > 0);
  });

  // Toplam ürün sayısı (Computed)
  totalProductsCount = computed(() => {
    return this.categories().reduce((sum, cat) => sum + cat.products.length, 0);
  });

  loadMenu(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.http.get<MenuResponse>(this.apiUrl).subscribe({
      next: (res) => {
        this.categories.set(res.categories || []);
        if (res.categories && res.categories.length > 0 && !this.activeCategoryId()) {
          this.activeCategoryId.set(res.categories[0].id);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Menü yüklenirken hata oluştu:', err);
        this.errorMessage.set('Menü yüklenemedi. Lütfen internet bağlantınızı kontrol edip tekrar deneyin.');
        this.isLoading.set(false);
      }
    });
  }

  setActiveCategory(categoryId: string): void {
    this.activeCategoryId.set(categoryId);
  }

  openProductDetail(product: Product): void {
    this.selectedProductForModal.set(product);
  }

  closeProductDetail(): void {
    this.selectedProductForModal.set(null);
  }
}
