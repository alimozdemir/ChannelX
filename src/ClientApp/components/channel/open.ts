import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { createDecorator } from 'vue-class-component'
import signalR, { HubConnection } from '@aspnet/signalr-client';
import axios from 'axios';
import resultModel from '../../models/resultModel';
import swal from 'sweetalert';
import { UserStore } from '../../stores/userState'
import UserComponent from "../user/user";
import './open.css';

interface getModel {
    id: number,
    title: string,
    endAt: Date,
    createdAt: Date,
    link: string
}

interface userDetail {
    UserId: string,
    ConnectionId: string,
    Name: string,
    GroupId: string,
    Authorized: boolean
}

interface  textModel{
    Content: string,
    User: userDetail | undefined,
    SentTime: Date
}

const defaultGetModel: getModel = { id: 0, title: "", endAt: new Date(), createdAt: new Date(), link : "" };
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
    user: userDetail | null = null;

    me(users: userDetail[]) {
        let index = users.findIndex(i => i.UserId === this.userId);

        if (index > -1)
            this.user = users[index];
        else
            this.user = null;

        console.log(this.user, index, this.userId, users)
    }
    toBottom() {

        let chat = document.getElementsByClassName('chat')[0];

        chat.scrollTop = chat.scrollHeight;
    }

    async fetchData() {
        console.log(this.userId);
        if (this.connection !== null) {
            this.connection.invoke('leave');
            this.connection.stop();
        }

        this.connection = null;
        this.model = Object.assign({}, defaultGetModel); //shallow copy of the default values

        let get;

        if (this.$route.params.id) {
            this.id = parseInt(this.$route.params.id);
            get = await axios.post('/api/Channel/Get', { id: this.id })
        }
        else if (this.$route.params.hash) {
            let hash = this.$route.params.hash;
            get = await axios.post('/api/Channel/GetHash', { id: hash })
        }
        else {
            swal({ text: "parameters is not valid", icon: 'error' });
            this.loading = false;
            return;
        }

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
                        this.id = this.model.id;
                        this.getLogs()
                    }
                    else {
                        swal({ text: result.message, icon: 'error' });
                    }
                }
            }
            else if (result.succeeded) {
                this.model = result.data as getModel;
                this.id = this.model.id;
                this.getLogs()
            }
            else {
                swal({ text: result.message, icon: 'error' });
            }
        }
    }

    async mounted() {

        this.loading = true;
        // due to file load order problem, 
        // we have to wait a little for loading axios settings.
        setTimeout(async () => { await this.fetchData(); }, 200)

    }

    async destroyed() {

        if (this.connection !== null) {
            await this.connection.invoke('leave');
            this.connection.stop();
        }
    }

    async getLogs() {
        this.userId = await UserStore.readUserId(this.$store);
        let url = chatAPI + UserStore.readAuthKey(this.$store);
        console.log(url)
        this.connection = new HubConnection(url, {});

        await this.connection.start();
        this.connection.on('userList', this.userList);
        this.connection.on('userLeft', this.userLeft);
        this.connection.on('userJoined', this.userJoined)
        this.connection.on('receive', this.receive)

        this.connection.invoke('join', { channelId: this.id });
        this.loading = false;
    }

    userList(users: userDetail[]) {
        this.me(users);
        this.users = users;
    }

    userLeft(user: userDetail) {
        let index = this.users.findIndex(i => i.ConnectionId == user.ConnectionId);
        if (index > -1) {
            let me = this.users.findIndex(i => i.UserId === this.userId);
            let model: textModel = { Content: this.users[index].Name + ' is left the channel.', User: undefined, SentTime: new Date() };
            this.users.splice(index, 1);
            this.chats.push(model)
        }
    }

    userJoined(user: userDetail) {
        let index = this.users.findIndex(i => i.UserId === user.UserId);

        if (index == -1) {
            let me = this.users.findIndex(i => i.UserId === this.userId);

            let model: textModel = { Content: user.Name + ` is join the channel.`, User: undefined, SentTime: new Date() };
            this.chats.push(model);
            this.users.push(user);
        }
    }

    send() {
        if (this.connection !== null && this.text !== "" && this.text !== undefined) {

            let me = this.users.findIndex(i => i.UserId === this.userId);

            let model: textModel = { Content: this.text, User: this.users[me], SentTime: new Date() };
            this.connection.invoke('send', model)

            this.text = "";
        }
    }

    receive(msg: textModel) {
        this.chats.push(msg);
    }

    async showUser(id: string) {
        let popup = new UserComponent(id);
        this.loading = true;
        await popup.show();
        this.loading = false;
    }

    async getSharableLink(){
        await swal(
            {
                title: this.model.title, 
                icon: "info",
                content: {
                    element: 'input',
                    attributes: {
                        value: this.model.link
                    }
                }
            });
    }
}
