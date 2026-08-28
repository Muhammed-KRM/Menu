import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { AdminCategory, AdminProduct, AdminProductDto } from '../../../../core/models/admin.model';

@Component({
  selector: 'app-product-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6">
      <!-- Başlık ve Yeni Ürün Ekle -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-[#E3D7C1] shadow-sm">
        <div>
          <h2 class="text-xl font-extrabold text-[#3A2418]">Ürün & Yemek Yönetimi</h2>
          <p class="text-xs text-[#725B4D]">Yemeklerin fiyatını, görselini, açıklamasını ve ait olduğu kategorileri (çoklu kategori) yönetin.</p>
        </div>
        <button 
          (click)="openModal()"
          class="px-5 py-2.5 bg-[#C65D3A] hover:bg-[#AA4B2B] text-white rounded-2xl font-bold text-xs shadow-md transition-all active:scale-95 flex items-center gap-2 w-fit cursor-pointer">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"></path>
          </svg>
          <span>Yeni Ürün Ekle</span>
        </button>
      </div>

      <!-- Ürünler Tablosu -->
      <div class="bg-white rounded-3xl border border-[#E3D7C1] overflow-hidden shadow-sm">
        @if (isLoading()) {
          <div class="py-16 text-center text-[#725B4D] text-xs font-semibold">Ürünler yükleniyor...</div>
        } @else if (products().length === 0) {
          <div class="py-16 text-center text-[#725B4D] text-xs">Henüz ürün eklenmedi.</div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm text-[#3A2418]">
              <thead class="text-xs uppercase bg-[#F7F1E3] text-[#725B4D] border-b border-[#E3D7C1]">
                <tr>
                  <th class="p-4">Görsel</th>
                  <th class="p-4">Ürün Adı & Açıklama</th>
                  <th class="p-4">Kategoriler (Çoklu)</th>
                  <th class="p-4">Fiyat</th>
                  <th class="p-4">Stok Durumu</th>
                  <th class="p-4 text-right">İşlemler</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-[#EFE7D5]">
                @for (prod of products(); track prod.id) {
                  <tr class="hover:bg-[#F7F1E3]/40 transition-colors">
                    <td class="p-4">
                      @if (prod.imageUrl) {
                        <img [src]="prod.imageUrl" alt="" class="w-12 h-12 rounded-2xl object-cover border border-[#E3D7C1]" />
                      } @else {
                        <div class="w-12 h-12 rounded-2xl bg-[#EFE7D5] flex items-center justify-center text-[#725B4D] text-xs">
                          🍲
                        </div>
                      }
                    </td>
                    <td class="p-4 max-w-xs">
                      <div class="font-bold text-[#3A2418] text-sm">{{ prod.name }}</div>
                      <div class="text-xs text-[#725B4D] line-clamp-1">{{ prod.description }}</div>
                    </td>
                    <td class="p-4">
                      <div class="flex flex-wrap gap-1">
                        @for (cat of prod.categories; track cat.id) {
                          <span class="px-2 py-0.5 rounded-full bg-[#3A2418]/10 text-[#3A2418] text-[11px] font-semibold">
                            {{ cat.name }}
                          </span>
                        }
                      </div>
                    </td>
                    <td class="p-4 font-black text-[#C65D3A] text-base">
                      {{ prod.price | currency:'TRY':'symbol-narrow':'1.0-2':'tr' }}
                    </td>
                    <td class="p-4">
                      <button 
                        (click)="toggleStock(prod)"
                        [class.bg-emerald-100]="prod.isAvailable"
                        [class.text-emerald-800]="prod.isAvailable"
                        [class.bg-red-100]="!prod.isAvailable"
                        [class.text-red-800]="!prod.isAvailable"
                        class="px-3 py-1 rounded-full text-xs font-bold transition-transform active:scale-95 cursor-pointer"
                        title="Tıklayarak stok durumunu değiştirin">
                        {{ prod.isAvailable ? '✓ Stokta Var' : '✕ Tükendi' }}
                      </button>
                    </td>
                    <td class="p-4 text-right space-x-2">
                      <button 
                        (click)="openModal(prod)"
                        class="p-2 text-[#3A2418] hover:bg-[#F7F1E3] rounded-xl transition-colors cursor-pointer"
                        title="Düzenle">
                        ✏️
                      </button>
                      <button 
                        (click)="deleteProduct(prod)"
                        class="p-2 text-red-600 hover:bg-red-50 rounded-xl transition-colors cursor-pointer"
                        title="Sil">
                        🗑️
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Ürün Ekle / Düzenle Modalı (Çoklu Kategori Seçimli) -->
      @if (showModal()) {
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
          <div class="bg-white rounded-3xl max-w-lg w-full max-h-[90vh] overflow-y-auto p-6 shadow-2xl border border-[#E3D7C1] relative animate-scale-up">
            <h3 class="text-xl font-extrabold text-[#3A2418] mb-4">
              {{ editingProductId() ? 'Ürünü Düzenle' : 'Yeni Ürün Ekle' }}
            </h3>

            <div class="space-y-4">
              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Ürün / Yemek Adı *</label>
                <input 
                  type="text" 
                  [(ngModel)]="formData.name"
                  placeholder="Örn: Trüflü Burger"
                  class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:border-[#C65D3A] focus:outline-none"
                />
              </div>

              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Açıklama / Malzemeler</label>
                <textarea 
                  [(ngModel)]="formData.description"
                  rows="2"
                  placeholder="İçerik, soslar ve sunum detayları..."
                  class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2 rounded-xl text-xs border border-[#E3D7C1] focus:outline-none"></textarea>
              </div>

              <!-- Çoklu Kategori Seçimi (Many-to-Many) -->
              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1.5">
                  Ait Olduğu Kategoriler (Birden fazla seçebilirsiniz) *
                </label>
                <div class="flex flex-wrap gap-2 p-3 bg-[#F7F1E3] rounded-2xl border border-[#E3D7C1] max-h-36 overflow-y-auto">
                  @for (cat of availableCategories(); track cat.id) {
                    <button 
                      type="button"
                      (click)="toggleCategorySelection(cat.id)"
                      [class.bg-[#3A2418]]="isCategorySelected(cat.id)"
                      [class.text-[#F7F1E3]]="isCategorySelected(cat.id)"
                      [class.border-[#3A2418]]="isCategorySelected(cat.id)"
                      [class.bg-white]="!isCategorySelected(cat.id)"
                      [class.text-[#3A2418]]="!isCategorySelected(cat.id)"
                      [class.border-[#E3D7C1]]="!isCategorySelected(cat.id)"
                      class="px-3 py-1.5 rounded-full text-xs font-semibold border transition-all cursor-pointer flex items-center gap-1.5">
                      <span>{{ isCategorySelected(cat.id) ? '✓' : '+' }}</span>
                      <span>{{ cat.name }}</span>
                    </button>
                  }
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Fiyat (TL) *</label>
                  <input 
                    type="number" 
                    [(ngModel)]="formData.price"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm font-bold border border-[#E3D7C1] focus:outline-none"
                  />
                </div>

                <div>
                  <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Kalori (kcal)</label>
                  <input 
                    type="number" 
                    [(ngModel)]="formData.calories"
                    placeholder="Örn: 650"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2.5 rounded-xl text-sm border border-[#E3D7C1] focus:outline-none"
                  />
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-[#725B4D] uppercase mb-1">Görsel Yükle / URL</label>
                <div class="flex gap-2 mb-2">
                  <input 
                    type="text" 
                    [(ngModel)]="formData.imageUrl"
                    placeholder="https://... veya dosya yükleyin"
                    class="w-full bg-[#F7F1E3] text-[#3A2418] px-4 py-2 rounded-xl text-xs border border-[#E3D7C1] focus:outline-none"
                  />
                  <label class="px-3 py-2 bg-[#3A2418] text-white rounded-xl text-xs font-bold hover:bg-[#2A180E] cursor-pointer whitespace-nowrap">
                    <span>Dosya Seç</span>
                    <input type="file" (change)="onFileSelected($event)" accept="image/*" class="hidden" />
                  </label>
                </div>
                @if (isUploading()) {
                  <p class="text-[11px] text-[#C65D3A] font-semibold">Görsel yükleniyor...</p>
                }
              </div>
            </div>

            <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-[#EFE7D5]">
              <button 
                (click)="closeModal()"
                class="px-4 py-2.5 text-xs font-bold text-[#725B4D] hover:bg-[#F7F1E3] rounded-xl transition-colors cursor-pointer">
                İptal
              </button>
              <button 
                (click)="saveProduct()"
                [disabled]="!formData.name || formData.price <= 0 || formData.categoryIds.length === 0"
                class="px-5 py-2.5 bg-[#C65D3A] hover:bg-[#AA4B2B] disabled:opacity-50 text-white rounded-xl font-bold text-xs transition-colors cursor-pointer">
                Kaydet
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class ProductManagerComponent implements OnInit {
  private adminService = inject(AdminService);

  products = signal<AdminProduct[]>([]);
  availableCategories = signal<AdminCategory[]>([]);
  isLoading = signal<boolean>(false);
  showModal = signal<boolean>(false);
  isUploading = signal<boolean>(false);
  editingProductId = signal<string | null>(null);

  formData: AdminProductDto = {
    name: '',
    description: '',
    price: 0,
    imageUrl: '',
    calories: undefined,
    preparationTime: '',
    isAvailable: true,
    categoryIds: []
  };

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.adminService.getProducts().subscribe({
      next: (data) => {
        this.products.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadCategories(): void {
    this.adminService.getCategories().subscribe({
      next: (data) => this.availableCategories.set(data)
    });
  }

  openModal(product?: AdminProduct): void {
    if (product) {
      this.editingProductId.set(product.id);
      this.adminService.getProductById(product.id).subscribe({
        next: (detail) => {
          this.formData = {
            name: detail.name,
            description: detail.description,
            price: detail.price,
            imageUrl: detail.imageUrl || '',
            calories: detail.calories,
            preparationTime: detail.preparationTime || '',
            isAvailable: detail.isAvailable,
            categoryIds: detail.categoryIds || []
          };
          this.showModal.set(true);
        }
      });
    } else {
      this.editingProductId.set(null);
      this.formData = {
        name: '',
        description: '',
        price: 0,
        imageUrl: '',
        calories: undefined,
        preparationTime: '',
        isAvailable: true,
        categoryIds: this.availableCategories().length > 0 ? [this.availableCategories()[0].id] : []
      };
      this.showModal.set(true);
    }
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  isCategorySelected(categoryId: string): boolean {
    return this.formData.categoryIds.includes(categoryId);
  }

  toggleCategorySelection(categoryId: string): void {
    const idx = this.formData.categoryIds.indexOf(categoryId);
    if (idx > -1) {
      this.formData.categoryIds.splice(idx, 1);
    } else {
      this.formData.categoryIds.push(categoryId);
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    this.isUploading.set(true);
    this.adminService.uploadImage(file).subscribe({
      next: (res) => {
        this.formData.imageUrl = `https://localhost:7000${res.imageUrl}`;
        this.isUploading.set(false);
      },
      error: () => {
        alert('Görsel yüklenirken bir hata oluştu.');
        this.isUploading.set(false);
      }
    });
  }

  saveProduct(): void {
    if (!this.formData.name || this.formData.price <= 0 || this.formData.categoryIds.length === 0) return;

    const id = this.editingProductId();
    if (id) {
      this.adminService.updateProduct(id, this.formData).subscribe({
        next: () => {
          this.closeModal();
          this.loadProducts();
        }
      });
    } else {
      this.adminService.createProduct(this.formData).subscribe({
        next: () => {
          this.closeModal();
          this.loadProducts();
        }
      });
    }
  }

  toggleStock(prod: AdminProduct): void {
    this.adminService.toggleProductStock(prod.id).subscribe({
      next: (res) => {
        prod.isAvailable = res.isAvailable;
      }
    });
  }

  deleteProduct(prod: AdminProduct): void {
    if (confirm(`"${prod.name}" ürününü silmek istediğinize emin misiniz?`)) {
      this.adminService.deleteProduct(prod.id).subscribe({
        next: () => this.loadProducts()
      });
    }
  }
}
