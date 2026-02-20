document.addEventListener('submit', async function (e) {
    var form = e.target;
    if (!form.classList.contains('favorite-form')) return;
    e.preventDefault();

    var button = form.querySelector('button[type="submit"]');
    button.disabled = true;

    try {
        var response = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form)
        });

        if (!response.ok) throw new Error();

        var result = await response.json();

        if (result.success) {
            var icon = button.querySelector('i');
            var label = button.querySelector('span');

            if (result.isFavorite) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill', 'text-danger');
                if (button.classList.contains('btn-favorite')) button.classList.add('active');
                if (label) label.textContent = 'Saved';
            } else {
                icon.classList.remove('bi-heart-fill', 'text-danger');
                icon.classList.add('bi-heart');
                if (button.classList.contains('btn-favorite')) button.classList.remove('active');
                if (label) label.textContent = 'Save';
            }
            showFavoriteToast(result.message, 'success');
        } else {
            showFavoriteToast(result.message || 'Action failed.', 'danger');
        }
    } catch (err) {
        showFavoriteToast('An error occurred. Please try again.', 'danger');
    } finally {
        button.disabled = false;
    }
});

function showFavoriteToast(message, type) {
    var existing = document.querySelector('.toast-notification');
    if (existing) existing.remove();

    var toast = document.createElement('div');
    toast.className = 'toast-notification toast-' + type;
    var iconClass = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';
    toast.innerHTML = '<i class="bi ' + iconClass + ' me-2"></i>' + message;
    document.body.appendChild(toast);

    requestAnimationFrame(function () {
        requestAnimationFrame(function () {
            toast.classList.add('show');
        });
    });

    setTimeout(function () {
        toast.classList.remove('show');
        setTimeout(function () { toast.remove(); }, 350);
    }, 3000);
}
