import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { createDecorator } from 'vue-class-component'
import signalR, { HubConnection } from '@aspnet/signalr-client';
import axios from 'axios';
import resultModel from '../../models/resultModel';
import swal from 'sweetalert';
import { UserStore } from '../../stores/userState'
import './open.css';

interface getModel {
    id: number,
    title: string,
    endAt: Date,
    createdAt: Date
}

interface userDetail {
    UserId: string,
    ConnectionId: string,
    Name: string,
    GroupId: string,
    Authorized: boolean
}

interface textModel {
    Content: string,
    User: string,
    Type: number,

}

const defaultGetModel: getModel = { id: 0, title: "", endAt: new Date(), createdAt: new Date() };
const chatAPI: string = "api/chat?token=";

@Component({
    watch: {
        // if user switch between channels, the vue router does not re-construct the component
        // thus, we have to call fetchData when the $route object changed
        '$route': 'fetchData',
        'chats': 'toBottom'
    }
})
export default class ChannelOpenComponent extends Vue {
    id: number = 0;
    model: getModel = Object.assign({}, defaultGetModel);
    text: string = "";
    loading: boolean = false;
    chats: textModel[] = [];
    connection: HubConnection | null = null;
    users: userDetail[] = [];
    userId: string = "";

    toBottom() {

        let chat = document.getElementsByClassName('chat')[0];

        chat.scrollTop = chat.scrollHeight;
    }

    async fetchData() {
        this.loading = true;
        if (this.connection !== null) {
            this.connection.invoke('leave');
            this.connection.stop();
        }

        this.connection = null;
        this.model = Object.assign({}, defaultGetModel); //shallow copy of the default values
        this.id = parseInt(this.$route.params.id);

        let get = await axios.post('/api/Channel/Get', { id: this.id })

        if (get.status == 200) {
            let result = get.data as resultModel;
            if (result.prompt) {

                var password = await swal({
                    text: result.message,
                    icon: "info",
                    content: {
                        element: 'input'
                    }
                });

                let getPassword = await axios.post('/api/Channel/GetWithPassword', { id: this.id, password: password });

                if (getPassword.status == 200) {
                    result = getPassword.data as resultModel;

                    if (result.succeeded) {
                        this.model = result.data as getModel;
                        this.getLogs()

                    }
                    else {
                        swal({ text: result.message, icon: 'error' });
                    }
                }
            }
            else if (result.succeeded) {
                this.getLogs()
            }
            else {
                swal({ text: result.message, icon: 'error' });
            }
        }
    }

    async mounted() {
        await this.fetchData();

        this.userId = UserStore.readUserId(this.$store);
    }

    async destroyed() {

        if (this.connection !== null) {
            await this.connection.invoke('leave');
            this.connection.stop();
        }
    }

    async getLogs() {
        let url = chatAPI + UserStore.readAuthKey(this.$store);
        console.log(url)
        this.connection = new HubConnection(url, {});

        await this.connection.start();
        this.loading = false;
        this.connection.on('userList', this.userList);
        this.connection.on('userLeft', this.userLeft);
        this.connection.on('userJoined', this.userJoined)
        this.connection.on('receive', this.receive)

        this.connection.invoke('join', { channelId: this.id });
    }

    userList(users: userDetail[]) {
        this.users = users;
    }

    userLeft(user: userDetail) {
        let index = this.users.findIndex(i => i.ConnectionId == user.ConnectionId);
        if (index > -1) {
            var model: textModel = { Content: this.users[index].Name + ' is left the channel.', Type: 3, User: 'Me' };
            this.users.splice(index, 1);
            this.chats.push(model)
        }
    }

    userJoined(user: userDetail) {
        let index = this.users.findIndex(i => i.ConnectionId === user.ConnectionId);

        if(index == -1) {
            var model: textModel = { Content: user.Name + ` is join the channel.`, Type: 3, User: 'Me' };
            this.chats.push(model);
            this.users.push(user);
        }
    }
    send() {
        if (this.connection !== null && this.text !== "" && this.text !== undefined) {
            var model: textModel = { Content: this.text, Type: 2, User: 'Me' };
            this.connection.invoke('send', model)

            this.text = "";
        }
    }
    receive(msg: textModel) {
        this.chats.push(msg);
    }
}
