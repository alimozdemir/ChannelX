import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';

@Component({
    components: {
        MenuComponent: require('../components/navmenu/navmenu.vue.html')
    }
})
export default class AppComponent extends Vue {
    async mounted(){
        var result = await axios.get('/Home/Test');
        if(result.status !== 200){
            this.$router.push('/login');
        }
    }
}
