

document.addEventListener('DOMContentLoaded', function() {
    const uploadForm = document.getElementById('uploadBannerForm');
    const settingsForm = document.getElementById('settingsForm');
    const editBannerForm = document.getElementById('editBannerForm');
    const bannerList = document.getElementById('bannerList');
    const livePreview = document.getElementById('livePreview');
    const refreshPreviewBtn = document.getElementById('refreshPreview');

    const linkType = document.getElementById('linkType');
    const linkTargetGroup = document.getElementById('linkTargetGroup');
    const linkTargetCategory = document.getElementById('linkTargetCategory');
    const linkTargetUrl = document.getElementById('linkTargetUrl');

    linkType.addEventListener('change', function() {
        updateLinkTargetVisibility(this.value, linkTargetGroup, linkTargetCategory, linkTargetUrl);
    });

    const editLinkType = document.getElementById('editLinkType');
    const editLinkTargetGroup = document.getElementById('editLinkTargetGroup');
    const editLinkTargetCategory = document.getElementById('editLinkTargetCategory');
    const editLinkTargetUrl = document.getElementById('editLinkTargetUrl');

    editLinkType.addEventListener('change', function() {
        updateLinkTargetVisibility(this.value, editLinkTargetGroup, editLinkTargetCategory, editLinkTargetUrl);
    });

    function updateLinkTargetVisibility(type, targetGroup, categorySelect, urlInput) {
        if (type === 'None') {
            targetGroup.style.display = 'none';
            categorySelect.style.display = 'none';
            urlInput.style.display = 'none';
            categorySelect.removeAttribute('name');
            urlInput.removeAttribute('name');
        } else if (type === 'Category') {
            targetGroup.style.display = 'block';
            categorySelect.style.display = 'block';
            urlInput.style.display = 'none';
            categorySelect.setAttribute('name', 'linkTarget');
            urlInput.removeAttribute('name');
        } else if (type === 'ExternalUrl') {
            targetGroup.style.display = 'block';
            categorySelect.style.display = 'none';
            urlInput.style.display = 'block';
            categorySelect.removeAttribute('name');
            urlInput.setAttribute('name', 'linkTarget');
        }
    }

    
    if (uploadForm) {
        uploadForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            const formData = new FormData(uploadForm);
            const submitBtn = uploadForm.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Laster opp...';

            try {
                const response = await fetch('/HomePageManagement/UploadBanner', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    showNotification('Banner lastet opp!', 'success');
                    uploadForm.reset();
                    linkTargetGroup.style.display = 'none';
                    refreshBannerList();
                    refreshPreview();
                } else {
                    showNotification(result.message || 'Feil ved opplasting', 'error');
                }
            } catch (error) {
                console.error('Upload error:', error);
                showNotification('Nettverksfeil', 'error');
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-upload"></i> Last opp banner';
            }
        });
    }

  
    let draggedElement = null;

    function setupDragAndDrop() {
        const bannerItems = bannerList.querySelectorAll('.hp-banner-item');

        bannerItems.forEach(item => {
            item.addEventListener('dragstart', handleDragStart);
            item.addEventListener('dragover', handleDragOver);
            item.addEventListener('drop', handleDrop);
            item.addEventListener('dragend', handleDragEnd);
        });
    }

    function handleDragStart(e) {
        draggedElement = this;
        this.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }

    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        
        const afterElement = getDragAfterElement(bannerList, e.clientY);
        if (afterElement == null) {
            bannerList.appendChild(draggedElement);
        } else {
            bannerList.insertBefore(draggedElement, afterElement);
        }
        
        return false;
    }

    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }
        return false;
    }

    function handleDragEnd(e) {
        this.classList.remove('dragging');
        updateBannerOrder();
    }

    function getDragAfterElement(container, y) {
        const draggableElements = [...container.querySelectorAll('.hp-banner-item:not(.dragging)')];

        return draggableElements.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;

            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }

   
    async function updateBannerOrder() {
        const bannerItems = bannerList.querySelectorAll('.hp-banner-item');
        const bannerIds = Array.from(bannerItems).map(item => 
            parseInt(item.dataset.bannerId)
        );

        try {
            const response = await fetch('/HomePageManagement/UpdateBannerOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(bannerIds)
            });

            const result = await response.json();

            if (result.success) {
                showNotification('Rekkefølge oppdatert', 'success');
                refreshPreview();
            } else {
                showNotification('Feil ved oppdatering', 'error');
                refreshBannerList();
            }
        } catch (error) {
            console.error('Order update error:', error);
            showNotification('Nettverksfeil', 'error');
        }
    }


 
    bannerList.addEventListener('click', async function(e) {
        const toggleBtn = e.target.closest('.toggle-active');
        if (toggleBtn) {
            const bannerId = toggleBtn.dataset.bannerId;
            
            try {
                const response = await fetch('/HomePageManagement/ToggleBannerActive', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: new URLSearchParams({
                        bannerId: bannerId,
                        __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value
                    })
                });

                const result = await response.json();

                if (result.success) {
                    showNotification(result.isActive ? 'Banner aktivert' : 'Banner deaktivert', 'success');
                    refreshBannerList();
                    refreshPreview();
                } else {
                    showNotification('Feil ved endring av status', 'error');
                }
            } catch (error) {
                console.error('Toggle error:', error);
                showNotification('Nettverksfeil', 'error');
            }
        }
    });

  
    bannerList.addEventListener('click', async function(e) {
        const deleteBtn = e.target.closest('.hp-delete-banner');
        if (deleteBtn) {
            if (!confirm('Er du sikker på at du vil slette dette banneret?')) {
                return;
            }

            const bannerId = deleteBtn.dataset.bannerId;
            
            try {
                const response = await fetch('/HomePageManagement/DeleteBanner', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: new URLSearchParams({
                        bannerId: bannerId,
                        __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value
                    })
                });

                const result = await response.json();

                if (result.success) {
                    showNotification('Banner slettet', 'success');
                    refreshBannerList();
                    refreshPreview();
                } else {
                    showNotification('Feil ved sletting', 'error');
                }
            } catch (error) {
                console.error('Delete error:', error);
                showNotification('Nettverksfeil', 'error');
            }
        }
    });

    
    bannerList.addEventListener('click', function(e) {
        const editBtn = e.target.closest('.edit-banner');
        if (editBtn) {
            const bannerId = editBtn.dataset.bannerId;
            const linkType = editBtn.dataset.linkType || 'None';
            const linkTarget = editBtn.dataset.linkTarget || '';
            
            document.getElementById('editBannerId').value = bannerId;
            
            editLinkType.value = linkType;
            
            if (linkType === 'Category') {
                editLinkTargetGroup.style.display = 'block';
                editLinkTargetCategory.style.display = 'block';
                editLinkTargetUrl.style.display = 'none';
                editLinkTargetCategory.value = linkTarget;
                editLinkTargetCategory.setAttribute('name', 'linkTarget');
                editLinkTargetUrl.removeAttribute('name');
            } else if (linkType === 'ExternalUrl') {
                editLinkTargetGroup.style.display = 'block';
                editLinkTargetCategory.style.display = 'none';
                editLinkTargetUrl.style.display = 'block';
                editLinkTargetUrl.value = linkTarget;
                editLinkTargetUrl.setAttribute('name', 'linkTarget');
                editLinkTargetCategory.removeAttribute('name');
            } else {
                editLinkTargetGroup.style.display = 'none';
                editLinkTargetCategory.style.display = 'none';
                editLinkTargetUrl.style.display = 'none';
                editLinkTargetCategory.removeAttribute('name');
                editLinkTargetUrl.removeAttribute('name');
            }
            
            $('#editBannerModal').modal('show');
        }
    });

   
    if (editBannerForm) {
        editBannerForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            const formData = new FormData(editBannerForm);
            const bannerId = formData.get('bannerId');
            const linkType = formData.get('linkType');
            let linkTarget = formData.get('linkTarget');

            if (linkType === 'Category') {
                linkTarget = editLinkTargetCategory.value;
            } else if (linkType === 'ExternalUrl') {
                linkTarget = editLinkTargetUrl.value;
            } else {
                linkTarget = '';
            }

            try {
                const response = await fetch('/HomePageManagement/UpdateBannerLink', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: new URLSearchParams({
                        bannerId: bannerId,
                        linkType: linkType,
                        linkTarget: linkTarget,
                        __RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value
                    })
                });

                const result = await response.json();

                if (result.success) {
                    showNotification('Banner-lenke oppdatert', 'success');
                    $('#editBannerModal').modal('hide');
                    refreshBannerList();
                    refreshPreview();
                } else {
                    showNotification('Feil ved oppdatering', 'error');
                }
            } catch (error) {
                console.error('Update error:', error);
                showNotification('Nettverksfeil', 'error');
            }
        });
    }

    
    if (settingsForm) {
        settingsForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            const formData = new FormData(settingsForm);
            const submitBtn = settingsForm.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Lagrer...';

            try {
                const response = await fetch('/HomePageManagement/UpdateSettings', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    showNotification('Innstillinger lagret', 'success');
                    refreshPreview();
                } else {
                    showNotification('Feil ved lagring', 'error');
                }
            } catch (error) {
                console.error('Settings error:', error);
                showNotification('Nettverksfeil', 'error');
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '<i class="fas fa-save"></i> Lagre innstillinger';
            }
        });
    }

  
    if (refreshPreviewBtn) {
        refreshPreviewBtn.addEventListener('click', refreshPreview);
    }

    function refreshPreview() {
        if (livePreview) {
            livePreview.src = livePreview.src;
        }
    }

 
    function refreshBannerList() {
        location.reload();
    }

 
    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 1rem 1.5rem;
            background-color: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6'};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 9999;
            animation: slideIn 0.3s ease;
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        @keyframes slideOut {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);

    setupDragAndDrop();
});
