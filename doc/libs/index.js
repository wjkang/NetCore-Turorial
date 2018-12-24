/**
 * Docsify config
 */
gitalkConfig = {
  clientID: '1d8b8fd94ef19d46e225',
  clientSecret: '2bf83dfdbd8d8a736a589ba3b4ed35bb766d7675',
  repo: 'i-notes',
  owner: 'wjkang',
  admin: ['wjkang'],
  distractionFreeMode: false
},
window.$docsify = {
  name: 'NetCore Turorial',
  repo: '',
  auto2top: true,
  loadSidebar: true,
  subMaxLevel: 5,
  homepage: 'README.md',
  search: {
    noData: {
      '/': '找不到结果!'
    },
    paths: 'auto',
    placeholder: {
      '/': '搜索'
    }
  },
  // plugins: [
  //   function(hook, vm) {
  //     hook.beforeEach(function (html) {
  //       var url = 'https://github.com/wjkang/i-notes/blob/master/' + vm.route.file;
  //       var editHtml = '[📝 EDIT DOCUMENT](' + url + ')\n';
        
  //       return editHtml + html;
  //     })

  //     hook.doneEach(function(){
  //       var label, domObj, main, divEle, gitalk;
  //       label = vm.route.path.split('/').join('');
  //       domObj = Docsify.dom;
  //       main = domObj.getNode("#main");

  //       /**
  //        * render gittalk
  //        */
  //       Array.apply(null,document.querySelectorAll("div.gitalk-container")).forEach(function(ele){ele.remove()});
  //       divEle = domObj.create("div");
  //       divEle.id = "gitalk-container-" + label;
  //       divEle.className = "gitalk-container";
  //       divEle.style = "width: " + main.clientWidth + "px; margin: 0 auto 20px;";
  //       domObj.appendTo(domObj.find(".content"), divEle);
  //       gitalk = new Gitalk(Object.assign(gitalkConfig, {id: !label ? "home" : label}))
  //       gitalk.render('gitalk-container-' + label)
  //     })
  //   }
  // ]
}