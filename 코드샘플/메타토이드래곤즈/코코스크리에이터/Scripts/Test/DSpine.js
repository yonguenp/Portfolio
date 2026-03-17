// v-prop="target.size" onclick="alert(this.parentNode.innerHTML)
// Editor.Panel.extend({
//       template: `<ui-button @confirm="onConfirm">Click Me</ui-button>`,

//       ready () {
//             new window.Vue({
//                   el: this.shadowRoot,
//                         methods: {
//                         onConfirm ( event ) {
//                               event.stopPropagation();
//                               console.log('On Confirm!');
//                         },
//                   },
//             });
//       },
// });