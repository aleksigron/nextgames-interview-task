import * as React from 'react';

import { Chat } from './Chat';

export class Home extends React.Component
{
	constructor(props)
	{
		super(props);

		this.onNameChange = this.onNameChange.bind(this);
		this.onSubmit = this.onSubmit.bind(this);

		this.state = {
			joined: false,
			name: ''
		};
	}

	onNameChange(e)
	{
		this.setState({ name: e.target.value });
	}

	onSubmit(e)
	{
		e.preventDefault();

		this.setState({ joined: true });
	}

	render()
	{
		return <div>
			<h1>Chat room</h1>
			{this.state.joined === false &&
				<form onSubmit={this.onSubmit}>
					<div>Username</div>
					<input type="text" value={this.state.name} onChange={this.onNameChange} />
					<input type="submit" value="Enter chat" />
				</form>
			}
			{this.state.joined === true && <Chat username={this.state.name}/> }
		</div>;
	}
}
