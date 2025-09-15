// 後台管理系統 JavaScript
$(document).ready(function() {
    // 側邊欄收合功能
    $('#toggleSidebar').click(function() {
        $('#sidebar').toggleClass('collapsed');
        
        // 儲存狀態到 localStorage
        if ($('#sidebar').hasClass('collapsed')) {
            localStorage.setItem('sidebarCollapsed', 'true');
        } else {
            localStorage.setItem('sidebarCollapsed', 'false');
        }
    });
    
    // 行動裝置側邊欄切換
    $('#toggleSidebarMobile').click(function() {
        $('#sidebar').toggleClass('show');
    });
    
    // 點擊外部關閉側邊欄（行動裝置）
    $(document).click(function(event) {
        if ($(window).width() < 992) {
            if (!$(event.target).closest('#sidebar, #toggleSidebarMobile').length) {
                $('#sidebar').removeClass('show');
            }
        }
    });
    
    // 載入側邊欄狀態
    if (localStorage.getItem('sidebarCollapsed') === 'true') {
        $('#sidebar').addClass('collapsed');
    }
    
    // 視窗大小改變時的處理
    $(window).resize(function() {
        if ($(window).width() >= 992) {
            $('#sidebar').removeClass('show');
        }
    });
    
    // 工具提示初始化
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
    
    // 表格排序功能
    $('.sortable').click(function() {
        var table = $(this).parents('table').eq(0);
        var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()));
        this.asc = !this.asc;
        if (!this.asc) { rows = rows.reverse(); }
        for (var i = 0; i < rows.length; i++) { table.append(rows[i]); }
        
        // 更新排序圖示
        $('.sortable i').removeClass('bi-caret-up-fill bi-caret-down-fill').addClass('bi-caret-down');
        if (this.asc) {
            $(this).find('i').removeClass('bi-caret-down').addClass('bi-caret-up-fill');
        } else {
            $(this).find('i').removeClass('bi-caret-down').addClass('bi-caret-down-fill');
        }
    });
    
    function comparer(index) {
        return function(a, b) {
            var valA = getCellValue(a, index), valB = getCellValue(b, index);
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB);
        }
    }
    
    function getCellValue(row, index) {
        return $(row).children('td').eq(index).text();
    }
    
    // 確認刪除提示
    $('.delete-confirm').click(function(e) {
        if (!confirm('確定要刪除嗎？此操作無法復原。')) {
            e.preventDefault();
        }
    });
    
    // 檔案上傳預覽
    $('input[type="file"]').change(function() {
        var files = this.files;
        var previewContainer = $(this).siblings('.image-preview-container');
        
        if (previewContainer.length === 0) {
            previewContainer = $('<div class="image-preview-container mt-2"></div>');
            $(this).after(previewContainer);
        }
        
        previewContainer.empty();
        
        for (var i = 0; i < files.length; i++) {
            if (files[i].type.match('image.*')) {
                var reader = new FileReader();
                reader.onload = (function(file) {
                    return function(e) {
                        var preview = $('<div class="image-preview-item d-inline-block me-2 mb-2 position-relative">' +
                            '<img src="' + e.target.result + '" class="img-thumbnail" style="width: 100px; height: 100px; object-fit: cover;">' +
                            '<small class="d-block text-center text-muted">' + file.name + '</small>' +
                            '</div>');
                        previewContainer.append(preview);
                    };
                })(files[i]);
                reader.readAsDataURL(files[i]);
            }
        }
    });
    
    // 載入動畫
    window.showLoading = function() {
        $('body').append('<div class="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">載入中...</span></div></div>');
    };
    
    window.hideLoading = function() {
        $('.loading-overlay').remove();
    };
    
    // AJAX 預設設定
    $.ajaxSetup({
        beforeSend: function() {
            showLoading();
        },
        complete: function() {
            hideLoading();
        }
    });
});

// 載入動畫樣式
$('<style>')
    .prop('type', 'text/css')
    .html(`
        .loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
        }
    `)
    .appendTo('head');