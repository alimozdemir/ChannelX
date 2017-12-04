import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { UserStore } from '../../stores/userState';


@Component
export default class HomeComponent extends Vue {
    mounted()
    {
        console.log(UserStore.readAuthKey(this.$store), this.$store);
        console.log(UserStore.readUserId(this.$store));
    }
}
