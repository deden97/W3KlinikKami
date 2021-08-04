// ketika div di click maka, <a href"#"> akan ter click.
function clickLinkByDiv(id) {
    document.getElementById(id).click();
}

// mengganti class saat suatu element di click
function changeClassName(arrId, activeId, nClass, nActClass) {
    for (let i = 0; i < arrId.length; i++) {
        document.getElementById(arrId[i]).className = (arrId[i] == activeId) ? nActClass : nClass;
    }
}

// checkbox untuk menampilkan password
function ckbShowPassword(checked, idTarget) {
    document.getElementById(idTarget).type = checked ? "text" : "password";
}

// ShowModalKw(id, type) dengan style.display
function ShowModalKw(id, type) {
    document.getElementById(id).style.display = type;
}

// membuka link di tab yg sama
function OpenLink(url) {
    location.href = url;
}

// membuka link di tab baru
function OpenLinkNewTab(url) {
    window.open(url, '_blank');
}

// submit data form
function SubmitForm(idForm) {
    document.querySelector(idForm).submit();
}