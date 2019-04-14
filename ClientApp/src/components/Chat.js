import * as React from 'react';

import * as signalR from '@aspnet/signalr';

export class Chat extends React.Component
{
	constructor(props)
	{
		super(props);
		
		this.onMessageTextChange = this.onMessageTextChange.bind(this);
		this.onSubmitMessage = this.onSubmitMessage.bind(this);

		this.onMessageReceived = this.onMessageReceived.bind(this);
		this.onUserJoined = this.onUserJoined.bind(this);
		this.onUserLeft = this.onUserLeft.bind(this);
		
		this.state = {
			joined: false,
			messageText: '',

			messages: [],
			users: [],

			hubConnection: undefined,
		};
	}

	componentDidMount()
	{
		const conn = new signalR.HubConnectionBuilder().withUrl('/hub').build();

		this.setState({ hubConnection: conn }, () =>
		{
			conn.on('MessageReceived', this.onMessageReceived);
			conn.on('UserJoined', this.onUserJoined);
			conn.on('UserLeft', this.onUserLeft);

			conn.start().then(() => conn.send('Join', this.props.username))
				.catch(err => console.log('Error connecting to SignalR hub: ' + err));
		});
	}

	onMessageReceived(messageSender, messageText)
	{
		const message = {
			sender: messageSender,
			text: messageText
		};

		this.setState({ messages: this.state.messages.concat(message) });
	}

	onUserJoined(usernames)
	{
		const modifyState = { users: this.state.users.concat(usernames)};

		// If we haven't joined yet and usernames contains our username, set state as joined
		if (this.state.joined === false && usernames.indexOf(this.props.username) !== -1)
			modifyState.joined = true;

		this.setState(modifyState);
	}

	onUserLeft(username)
	{
		const newUsers = this.state.users.filter(val => val !== username);
		this.setState({ users: newUsers });
	}

	onMessageTextChange(e)
	{
		this.setState({ messageText: e.target.value });
	}

	onSubmitMessage(e)
	{
		e.preventDefault();

		this.state.hubConnection.send('SendMessage', this.state.messageText)
			.then(() => this.setState({ message: '' }));
	}

	render()
	{
		return <div>
			<form onSubmit={this.onSubmitMessage}>
				<input type="text" value={this.state.messageText} onChange={this.onMessageTextChange} />
				<input type="submit" value="Send message" />
			</form>
			<p>Username: {this.props.username}</p>
			<p>Joined: {this.state.joined.toString()}</p>
			<h2>Online users</h2>
			{this.state.users.map(username => <div key={username}>{username}</div>)}
			<h2>Messages</h2>
			{this.state.messages.map((message, index) => <div key={index}>{message.sender}: {message.text}</div>)}
		</div>;
	}
}
